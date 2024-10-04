# MediaPipe Body
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from clientUDP import ClientUDP

import cv2
import threading
import time
import global_vars 
import struct
import numpy as np
import sys

from pyorbbecsdk import *
from utils import frame_to_bgr_image
from mp_depth import TemporalFilter

ESC_KEY = 27
class StoppableThread(threading.Thread):
    """Thread class with a stop() method. The thread itself has to check
    regularly for the stopped() condition."""

    def __init__(self,  *args, **kwargs):
        super(StoppableThread, self).__init__(*args, **kwargs)
        self._stop_event = threading.Event()

    def stop(self):
        self._stop_event.set()

    def stopped(self):
        return self._stop_event.is_set()
    
class CaptureThreadOrbbec(StoppableThread):
    pipeline = None
    align_filter = None
    temporal_filter = TemporalFilter(alpha=global_vars.ALPHA_TEMPORAL)
    color_frame = None
    depth_frame = None
    color_image = None
    depth_data = None
    isRunning = False
    counter = 0
    timer = 0.0

    def stop(self):
        self.pipeline.stop()
        self._stop_event.set()
    def run(self):
        self.pipeline = Pipeline()
        config = Config()

        try:
            # Set up color stream
            profile_list = self.pipeline.get_stream_profile_list(OBSensorType.COLOR_SENSOR)
            color_profile = profile_list.get_video_stream_profile(1280, 720, OBFormat.MJPG, 15)
            config.enable_stream(color_profile)

            # Set up depth stream
            profile_list = self.pipeline.get_stream_profile_list(OBSensorType.DEPTH_SENSOR)
            depth_profile = profile_list.get_default_video_stream_profile()
            config.enable_stream(depth_profile)

            # Enable frame sync
            self.pipeline.enable_frame_sync()

            self.align_filter = AlignFilter(align_to_stream=OBStreamType.COLOR_STREAM)

        except Exception as e:
            print(f"Error setting up the Orbbec camera: {e}")
            return

        # Start the pipeline
        try:
            self.pipeline.start(config)
        except Exception as e:
            print(f"Error starting the pipeline: {e}")
            return

        time.sleep(1)  # Allow the pipeline to warm up
        print("Opened Orbbec Capture @ 30 fps")
        
        self.isRunning = True
        self.timer = time.time()

        while not global_vars.KILL_THREADS:
            try:
                frames = self.pipeline.wait_for_frames(100)
                if not frames:
                    continue

                self.color_frame = frames.get_color_frame()
                self.depth_frame = frames.get_depth_frame()
                if not self.color_frame or not self.depth_frame:
                    continue

                # Align and process frames
                frames = self.align_filter.process(frames)
                self.color_frame = frames.get_color_frame()
                self.depth_frame = frames.get_depth_frame()

                # Convert color frame to BGR image
                self.color_image = frame_to_bgr_image(self.color_frame)
                if self.color_image is None:
                    print("Failed to convert frame to image")
                    continue

                # Convert depth frame to numpy array
                self.depth_data = np.frombuffer(self.depth_frame.get_data(), dtype=np.uint16).reshape(
                    (self.depth_frame.get_height(), self.depth_frame.get_width())
                )
                self.depth_data = self.depth_data.astype(np.float32) * self.depth_frame.get_depth_scale()
                self.depth_data = self.temporal_filter.process(self.depth_data)

                # FPS Calculation (Optional)
                if global_vars.DEBUG:
                    self.counter += 1
                    if time.time() - self.timer >= 3:
                        print(f"Capture FPS: {self.counter / (time.time() - self.timer)}")
                        self.counter = 0
                        self.timer = time.time()
            except KeyboardInterrupt:
                self.pipeline.stop()
                sys.exit()

# Update BodyThread to use depth data for z-coordinate adjustments
class BodyThreadOrbbec(StoppableThread):
    data = ""
    pipe = None
    timeSinceCheckedConnection = 0
    timeSincePostStatistics = 0

    def run(self):
        mp_drawing = mp.solutions.drawing_utils
        mp_pose = mp.solutions.pose

        self.setup_comms()

        capture = CaptureThreadOrbbec()
        capture.start()

        with mp_pose.Pose(min_detection_confidence=0.80, min_tracking_confidence=0.5, model_complexity=global_vars.MODEL_COMPLEXITY, static_image_mode=False, enable_segmentation=True) as pose:
            orbbec_wait = 0
            while not global_vars.KILL_THREADS and capture.isRunning == False:
                try:
                    if orbbec_wait < global_vars.ORBBEC_CONNECTION_TIMEOUT:
                        orbbec_wait += 1
                        print("Waiting for Orbbec camera and capture thread.")
                        time.sleep(1)
                    else:
                        print("Timeout trying to connect to the Orbbec, check the connections to the camera and try again")
                        sys.exit()
                except KeyboardInterrupt:
                    capture.stop()
                    sys.exit()
            print("Beginning capture")

            while not global_vars.KILL_THREADS:
                try:
                    ti = time.time()

                    # Fetch data from the capture thread
                    color_image = capture.color_image
                    depth_data = capture.depth_data

                    # Image transformations and processing
                    color_image = cv2.flip(color_image, 1)
                    color_image.flags.writeable = global_vars.DEBUG

                    # MediaPipe Pose detections
                    results = pose.process(color_image)
                    tf = time.time()

                    # Rendering results
                    if global_vars.DEBUG:
                        if time.time() - self.timeSincePostStatistics >= 1:
                            print("Theoretical Maximum FPS: %f" % (1 / (tf - ti)))
                            self.timeSincePostStatistics = time.time()

                        if results.pose_landmarks:
                            mp_drawing.draw_landmarks(color_image, results.pose_landmarks, mp_pose.POSE_CONNECTIONS,
                                                    mp_drawing.DrawingSpec(color=(255, 100, 0), thickness=2, circle_radius=4),
                                                    mp_drawing.DrawingSpec(color=(255, 255, 255), thickness=2, circle_radius=2))
                        cv2.imshow('Body Tracking', color_image)
                        if cv2.waitKey(3) in [ord('q'), ESC_KEY]:
                            break
                        # cv2.waitKey(3)

                    
                    if results.pose_landmarks:
                        # Adjust z-coordinates based on depth data
                        landmarks = results.pose_landmarks.landmark
                        self.data = ""
                        i = 0
                        for lm in landmarks:
                            if 0 <= lm.x <= 1 and 0 <= lm.y <= 1:
                                x_pixel = int(lm.x * color_image.shape[1])
                                y_pixel = int(lm.y * color_image.shape[0])
                                depth_value = depth_data[y_pixel, x_pixel]
                                lm.z = depth_value / 1000.0  # Converting to meters
                            # Prepare data to send to Unity
                            self.data += "{}|{}|{}|{}\n".format(i,landmarks[i].x,landmarks[i].y,landmarks[i].z)
                            i += 1
                        # Send processed data
                        self.send_data(self.data)
                except KeyboardInterrupt:
                    capture.stop()
                    break

        capture.pipeline.stop()
        cv2.destroyAllWindows()
        capture.stop()
        pass

    def setup_comms(self):
        if not global_vars.USE_LEGACY_PIPES:
            self.client = ClientUDP(global_vars.HOST,global_vars.PORT)
            self.client.start()
        else:
            print("Using Pipes for interprocess communication (not supported on OSX or Linux).")
        pass      

    def send_data(self,message):
        if not global_vars.USE_LEGACY_PIPES:
            self.client.sendMessage(message)
            pass
        else:
            # Maintain pipe connection.
            if self.pipe==None and time.time()-self.timeSinceCheckedConnection>=1:
                try:
                    self.pipe = open(r'\\.\pipe\UnityMediaPipeBody1', 'r+b', 0)
                except FileNotFoundError:
                    print("Waiting for Unity project to run...")
                    self.pipe = None
                self.timeSinceCheckedConnection = time.time()

            if self.pipe != None:
                try:     
                    s = self.data.encode('utf-8') 
                    self.pipe.write(struct.pack('I', len(s)) + s)   
                    self.pipe.seek(0)    
                except Exception as ex:  
                    print("Failed to write to pipe. Is the unity project open?")
                    self.pipe= None
        pass
                        