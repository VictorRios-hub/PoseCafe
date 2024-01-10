# LivePose Configs

This folder contains LivePose configs which we will use to create real time interaction between a closeby watcher and the display.

## 14/06/2023

Added files necessary for first visual prototype of PoseCafe. This includes: 

- posecafe.py : the dimmap to associate 3d coordinates to the keypoints that are detected in the rgb frame. Simply add this in the dimmap folder of LivePose before running the config file.

- posecafefilter.py: the filter associated to our specific case of usage. It currently discriminates keypoints that are too far from the camera (to avoid spurious hand detections far away), but can be eventually modified so that we only send out osc messages about the wrist location, for example. 

- test_posecafe.json: the config file with which we want to run LivePose. (Add it to the config files of livepose, and run livepose using the command line ` ./livepose.sh -f -c livepose/configs/test_posecafe.json`)


