from typing import Any, List

from livepose.dataflow import Channel, Flow, Stream
from livepose.filter import register_filter, Filter
from livepose.pose_backend import PosesForCamera
import numpy as np

@register_filter("posecafefilter")
class PoseCafeFilter(Filter):
    """
    Filter which outputs the full skeleton for each person detected by camera, based on
    the BODY_25 dataset.
    For more information about the bones, see:
    https://github.com/CMU-Perceptual-Computing-Lab/openpose/blob/master/doc/output.md#keypoint-ordering

    OSC message is as follows, for each person and body part:
    {base OSC path}/skeleton/{camera_id}/{Person ID}/{Body Part} {screen X position} {screen Y position} {confidence value}
    """

    def __init__(self, *args: Any, **kwargs: Any):
        super(PoseCafeFilter, self).__init__("posecafefilter", **kwargs)

    def add_parameters(self) -> None:
        super().add_parameters()
        self._parser.add_argument("--min-pose-completeness", type=float, default=0.0, help="Minimum pose completeness")
        self._parser.add_argument("--two-dimensional", type=bool, default=True, help="Two dimensional")
        self._parser.add_argument("--three-dimensional", type=bool, default=False, help="Three dimensional")

    def init(self) -> None:

        self._show_2d_poses = self._args.two_dimensional
        self._show_3d_poses = self._args.three_dimensional
        self._min_pose_completeness = self._args.min_pose_completeness

    def step(self, flow: Flow, now: float, dt: float) -> None:
        """
        Update the filter
        :param flow: Flow - Data flow to read from and write to
        :param now: Current time
        :param dt: Time since last call
        """
        super().step(flow=flow, now=now, dt=dt)

        result: Filter.Result = {}

        # Get all pose streams
        pose_streams = flow.get_streams_by_type(Stream.Type.POSE_BACKEND)
        pose_streams.update(flow.get_streams_by_type(Stream.Type.DIMMAP))

        # All poses from all streams are sent
        for stream in pose_streams.values():
            assert(stream is not None)

            channels: List[Channel] = []
            if self._show_2d_poses:
                channels += stream.get_channels_by_type(Channel.Type.POSE_2D)
            if self._show_3d_poses:
                channels += stream.get_channels_by_type(Channel.Type.POSE_3D)

            for channel in channels:
                poses_for_cameras: List[PosesForCamera] = channel.data

                for cam_id, poses_for_camera in enumerate(poses_for_cameras):
                    result[cam_id] = {}

                    # initialize min and max z coords
                    min_z_coord = 0.0
                    max_z_coord = 2.0
                    # find all the keypoints that are within a specified depth range
                    # if 2 keypoints are detected within the range, chose the most probable
                    # but return a warning that the specified range might need to be tighter
                    for pose_index, pose in enumerate(poses_for_camera.poses):

                        # Iter over each keypoint.
                        for name, keypoint in pose.keypoints.items():
                            if keypoint is not None:
                                coords = keypoint.position
                                #confidence = keypoint.confidence
                                dist = coords[2]

                                #if confidence > 0.0: 
                                if min_z_coord <= dist and dist < max_z_coord:

                                    result[cam_id][keypoint.part] = [
                                        *coords[0:2]
                                    ]

        self._result = result

        if not flow.has_stream(self._filter_name):
            flow.add_stream(name=self._filter_name, type=Stream.Type.FILTER)

        flow.set_stream_frame(
            name=self._filter_name,
            frame=[Channel(
                type=Channel.Type.OUTPUT,
                data=self._result,
                name=self._filter_name,
                metadata={}
            )]
        )
