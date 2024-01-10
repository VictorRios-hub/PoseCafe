import numpy as np

from typing import Any, Dict, List, Optional, Tuple
import numpy.typing as npt

from dataclasses import dataclass

from livepose.dataflow import Channel, Flow, Stream
from livepose.dimmap import register_dimmap, DimMap
from livepose.pose_backend import PosesForCamera, Pose, Keypoint

import logging

logger = logging.getLogger(__name__)


@dataclass
class IntrinsicsParams:
    def __init__(self, width: int, height: int, fx: float, fy: float, ppx: float, ppy: float) -> None:
        self.width = width  # frame size (in pixel units)
        self.height = height
        self.fx = fx  # field of view (in pixel units)
        self.fy = fy
        self.ppx = ppx  # optical center of the camera (in pixel units)
        self.ppy = ppy

    width: int
    height: int
    fx: float
    fy: float
    ppx: float
    ppy: float


@register_dimmap("posecafe")
class PoseCafe(DimMap):
    """
    Dimension mapping which uses the depth map from a generic stereo camera to convert 2D poses
    to 3D, in the reference frame of the camera. All units end up being in meters. Based on the projection
    method from realsense (https://github.com/IntelRealSense/librealsense/blob/4f37f2ef0874c1716bce223b20e46d00532ffb04/include/librealsense2/rsutil.h). 
    Currently assumes no distorsion is present in the camera. 

    Based on generic_2d_to_3d_dimmap. Specific tweaks for PoseCafé project in compute_position function (mostly regarding use of mediapipe backend).
    """

    def __init__(self, *args: Any, **kwargs: Any):
        super(PoseCafe, self).__init__("posecafe", **kwargs)

    def add_parameters(self) -> None:
        super().add_parameters()
        self._parser.add_argument("--calibration-matrices", default=[],  # type: ignore
                                  help="Calibration matrices")
        self._parser.add_argument("--color-intrinsics", default=dict(),
                                  help="Intrinsics parameters for rgb camera")
        self._parser.add_argument("--depth-intrinsics", default=dict(),
                                  help="Intrinsics parameters for depth camera")
        self._parser.add_argument("--color-to-depth", default=[np.zeros([4, 3])],
                                  help="Extrinsics parameters from rgb to depth camera")
        self._parser.add_argument("--depth-scale", type=float, default=0.001,
                                  help="depth scale to convert distance in meters")
        self._parser.add_argument("--depth-min", type=float, default=0.1,
                                  help="minimal distance used for projection (in meters)")
        self._parser.add_argument("--depth-max", type=float, default=3.0,
                                  help="maximal distance used for projection (in meters)")

    def init(self) -> None:
        # obtain rgb intrinsics dict
        color_dict: Dict[str, Any] = self._args.color_intrinsics
        self._color_intrinsics: IntrinsicsParams = IntrinsicsParams(
            width=color_dict["width"],
            height=color_dict["height"],
            fx=color_dict["fx"],
            fy=color_dict["fy"],
            ppx=color_dict["ppx"],
            ppy=color_dict["ppy"])

        # obtain depth intrinsics dict
        depth_dict: Dict[str, Any] = self._args.depth_intrinsics
        self._depth_intrinsics: IntrinsicsParams = IntrinsicsParams(
            width=depth_dict["width"],
            height=depth_dict["height"],
            fx=depth_dict["fx"],
            fy=depth_dict["fy"],
            ppx=depth_dict["ppx"],
            ppy=depth_dict["ppy"])

        # obtain rgb to depth [R|T] matrix
        self._color_to_depth: npt.NDArray = np.squeeze(np.array(
            self._args.color_to_depth), axis=0)  # squeeze to remove useless dimension

        # obtain depth to rgb [R|T] matrix
        # extract rotation matrix from color to depth extrinsics
        R = self._color_to_depth[0:3, :]
        # extract translation vector from color to depth extrinsics
        T = self._color_to_depth[3, :]

        # depth_to color extrinsics are obtained from the transpose of the rotation matrix and the inverse of the translation vector
        self._depth_to_color: npt.NDArray = np.append(
            np.transpose(R), [-T], axis=0)

        # obtain depth projection parameters
        self._depth_scale: float = self._args.depth_scale
        self._depth_min: float = self._args.depth_min
        self._depth_max: float = self._args.depth_max

    def project_point_to_pixel(self, intrinsics: IntrinsicsParams, point: npt.NDArray) -> npt.NDArray:
        """ Given a point in 3D space, compute the corresponding pixel coordinates in an image produced by the same camera """

        x = point[0] / point[2]
        y = point[1] / point[2]

        pixel = np.array([x * intrinsics.fx + intrinsics.ppx,
                         y * intrinsics.fy + intrinsics.ppy])

        return pixel

    def deproject_pixel_to_point(self, pixel: npt.NDArray, intrinsics: IntrinsicsParams, depth: float) -> npt.NDArray:
        """ Given pixel coords and depth in an image, compute the corresponding point in 3D space relative to the camera"""

        x = (pixel[0] - intrinsics.ppx) / intrinsics.fx
        y = (pixel[1] - intrinsics.ppy) / intrinsics.fy

        point = np.array([depth * x, depth * y, depth])

        return point

    def transform_point_to_point(self, extrinsics: npt.NDArray, point: npt.NDArray) -> npt.NDArray:
        "Given a point in 3d space in reference to one camera, apply transformation matrix to find coordinates with reference to another"

        # add unit index to include translation parameters in the transformation
        point_add_t = np.append(point, [1.])

        new_pt = np.dot(np.transpose(extrinsics), point_add_t)

        return new_pt

    def next_pixel_in_line(self, p: npt.NDArray, start: npt.NDArray, end: npt.NDArray) -> npt.NDArray:
        """ Calculates the next pseudo pixel in the line of depth """
        lineslope = (end[1] - start[1]) / (end[0] - start[0])

        if end[0] == start[0]:
            if end[1] > p[1]:
                p[1] += 1
            else:
                p[1] -= 1

        elif end[1] == start[1]:  # this is the main case that we'll deal with when using realsense cameras
            if end[0] > p[0]:
                p[0] += 1
            else:
                p[0] -= 1
        else:
            if abs(end[0] - p[0]) > abs(end[1] - p[1]):

                if end[0] > p[0]:
                    p[0] += 1
                else:
                    p[0] -= 1

                p[1] = end[1] - lineslope * (end[0] - p[0])

            else:

                if end[1] > p[1]:
                    p[1] += 1
                else:
                    p[1] -= 1

                p[0] = end[0] - (end[1] + p[1]) / lineslope

        return p

    def is_pixel_in_line(self, p: npt.NDArray, start: npt.NDArray, end: npt.NDArray) -> bool:
        """ Returns a bool whether the computed pixel is in line or not"""

        return (((end[0] >= start[0] and end[0] >= p[0] and p[0] >= start[0]) or (end[0] <= start[0] and end[0] <= p[0] and p[0] <= start[0])) and
                ((end[1] >= start[1] and end[1] >= p[1] and p[1] >= start[1]) or (end[1] <= start[1] and end[1] <= p[1] and p[1] <= start[1])))

    def adjust_2D_point_to_boundary(self, intrinsics: IntrinsicsParams, pixel: npt.NDArray, ) -> npt.NDArray:
        """ In case the projected pixel computed lies outside the width/height boundary"""

        if pixel[0] < 0:
            pixel[0] = 0
        if pixel[0] > intrinsics.width:
            pixel[0] = intrinsics.width
        if pixel[1] < 1:
            pixel[1] = 0
        if pixel[1] > intrinsics.height:
            pixel[1] = intrinsics.height

        return pixel

    def project_color_pixel_to_depth_pixel(self, depth_frame: npt.NDArray, depth_scale: float, depth_min: float, depth_max: float, pixel: npt.NDArray) -> npt.NDArray:
        """ Find projected pixel with unknown depth search along line """

        # Find line start pixel

        min_point = self.deproject_pixel_to_point(
            pixel, self._color_intrinsics, depth_min)
        min_transformed_point = self.transform_point_to_point(
            self._color_to_depth, min_point)
        start_pixel = self.project_point_to_pixel(
            self._depth_intrinsics, min_transformed_point)
        start_pixel = self.adjust_2D_point_to_boundary(
            self._depth_intrinsics, start_pixel)
        start_pixel = np.rint(start_pixel).astype(int)

        # Find line end pixel
        max_point = self.deproject_pixel_to_point(
            pixel, self._color_intrinsics, depth_max)
        max_transformed_point = self.transform_point_to_point(
            self._color_to_depth, max_point)
        end_pixel = self.project_point_to_pixel(
            self._depth_intrinsics, max_transformed_point)
        end_pixel = self.adjust_2D_point_to_boundary(
            self._depth_intrinsics, end_pixel)
        end_pixel = np.rint(end_pixel).astype(int)

        # Search along line for the depth pixel that has the closest projection to the input pixel

        min_dist: float = -1.0  # initialize while loop

        p: npt.NDArray = start_pixel

        while self.is_pixel_in_line(p, start_pixel, end_pixel):

            p = np.rint(p).astype(int)
            depth = depth_scale * depth_frame[p[1]][p[0]]

            point = self.deproject_pixel_to_point(
                p, self._depth_intrinsics, depth)
            transformed_point = self.transform_point_to_point(
                self._depth_to_color, point)
            projected_pixel = self.project_point_to_pixel(
                self._color_intrinsics, transformed_point)

            new_dist = pow(
                (projected_pixel[1] - pixel[1]), 2) + pow((projected_pixel[0] - pixel[0]), 2)

            if new_dist < min_dist or min_dist < 0:
                min_dist = new_dist
                depth_pixel = p

            p = self.next_pixel_in_line(p, start_pixel, end_pixel)

        return depth_pixel

    def compute_position(self, cam_id: int, color_frames: List[Channel], depth_frames: List[Channel], pt: npt.NDArray) -> npt.NDArray:
        """
        Compute the depth from screen points.
        :param frames: List[Dict[str, Any]] - List of all input video frames
        """
        color_frame = color_frames[cam_id].data
        depth_frame = depth_frames[cam_id].data

        # If necessary, apply flipping and rotation to the input points, as the camera frames have
        # not been transformed (they are still the raw channel values)
        if color_frames[cam_id].metadata["flip"]:
            pt[0] = color_frames[cam_id].metadata["resolution"][0] - pt[0]

        if color_frames[cam_id].metadata["rotate"] != 0:
            angle = color_frames[cam_id].metadata["rotate"]
            res_x = color_frames[cam_id].metadata["resolution"][0]
            res_y = color_frames[cam_id].metadata["resolution"][1]

            if angle == 90:
                pt = npt.NDArray([
                    res_x - (pt[1] - res_y / 2),
                    (pt[0] - res_x / 2) + res_y
                ])
            elif angle == 180:
                pt = npt.NDArray([
                    res_x - (pt[0] - res_x / 2),
                    res_y - (pt[1] - res_y / 2)
                ])
            elif angle == 270:
                pt = npt.NDArray([
                    (pt[1] - res_y / 2) - res_x,
                    res_y - (pt[0] - res_x / 2)
                ])

        depth_ptr = self.project_color_pixel_to_depth_pixel(
            depth_frame=depth_frame,
            depth_scale=self._depth_scale,
            depth_min=self._depth_min,
            depth_max=self._depth_max,
            pixel=pt*np.array([640,480])
        )
        ptr = np.rint(depth_ptr).astype(int)

        if ptr[0] < 0 or ptr[0] > self._depth_intrinsics.width:
            return None  # type: ignore

        if ptr[1] < 0 or ptr[1] > self._depth_intrinsics.height:
            return None  # type: ignore

        distance = self._depth_scale*depth_frame[ptr[1]][ptr[0]]

        if distance == 0.0:
            return None  # type: ignore

        pt_with_dist = np.append(pt, distance)
        print(pt_with_dist)
        return pt_with_dist

    def step(self, flow: Flow, now: float, dt: float) -> None:
        """
        Process the given image(s) with the DL Backend model
        :param flow: Flow - Data flow to read from and write to
        :param now: float - current time
        :param dt: float - time since last call
        :return: bool - success
        """
        super().step(flow=flow, now=now, dt=dt)

        poses_3d_by_camera: List[PosesForCamera] = []

        # Get input frames
        color_frames: List[Channel] = []
        depth_frames: List[Channel] = []
        input_streams = flow.get_streams_by_type(Stream.Type.INPUT)
        for stream in input_streams.values():
            if stream.has_channel_type(Channel.Type.COLOR):
                color_frames.append(
                    stream.get_channels_by_type(Channel.Type.COLOR)[0])
            if stream.has_channel_type(Channel.Type.DEPTH):
                depth_frames.append(
                    stream.get_channels_by_type(Channel.Type.DEPTH)[0])

        pose_streams = flow.get_streams_by_type(Stream.Type.POSE_BACKEND)

        for stream in pose_streams.values():
            assert(stream is not None)
            for channel in stream.get_channels_by_type(Channel.Type.POSE_2D):
                poses_for_cameras: List[PosesForCamera] = channel.data

                for cam_id, poses_for_camera in enumerate(poses_for_cameras):
                    poses_3d: List[Pose] = []
                    for pose in poses_for_camera.poses:
                        keypoints_3d: Dict[str, Keypoint] = {}

                        for name, keypoint in pose.keypoints.items():
                            np.r_[np.array(keypoint.position)]
                            keypoint_3d = self.compute_position(
                                cam_id, color_frames, depth_frames, np.r_[np.array(keypoint.position)])

                            if keypoint_3d is None:
                                continue

                            if cam_id < len(self._args.calibration_matrices):
                                keypoint_3d = tuple(
                                    np.dot(np.r_[keypoint_3d, 1.0], np.r_[
                                           self._args.calibration_matrices[cam_id]])  # type: ignore
                                )[0:3]

                            keypoints_3d[name] = Keypoint(
                                confidence=keypoint.confidence,
                                part=name,
                                position=list(keypoint_3d)
                            )

                        pose_3d: Pose = Pose(
                            confidence=pose.confidence,
                            keypoints=keypoints_3d,
                            id=pose.id,
                            keypoints_definitions=pose.keypoints_definitions
                        )

                        poses_3d.append(pose_3d)

                    poses_3d_by_camera.append(PosesForCamera(poses=poses_3d))

        if not flow.has_stream(self.dimmap_name):
            flow.add_stream(name=self.dimmap_name, type=Stream.Type.DIMMAP)

        flow.set_stream_frame(
            name=self.dimmap_name,
            frame=[Channel(
                type=Channel.Type.POSE_3D,
                data=poses_3d_by_camera,
                name=self.dimmap_name,
                metadata={}
            )]
        )
