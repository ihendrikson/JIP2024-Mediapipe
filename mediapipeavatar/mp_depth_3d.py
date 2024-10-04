import argparse
import sys
import cv2
import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from matplotlib.widgets import Slider  # Import Slider for adding a slider
from pyorbbecsdk import *
from utils import frame_to_bgr_image
import mediapipe as mp

ESC_KEY = 27

# Temporal filter for smoothing depth data over time
class TemporalFilter:
    def __init__(self, alpha=0.5):
        self.alpha = alpha
        self.previous_frame = None

    def process(self, frame):
        if self.previous_frame is None:
            self.previous_frame = frame
            return frame
        result = cv2.addWeighted(frame, self.alpha, self.previous_frame, 1 - self.alpha, 0)
        self.previous_frame = result
        return result

def plot_3d_landmarks(landmarks, ax):
    x_vals = [lm.x for i, lm in enumerate(landmarks) if i not in range(1,10) and i not in range(17,22)]
    z_vals = [(-lm.y + 1) for i, lm in enumerate(landmarks) if i not in range(1,10) and i not in range(17,22)]
    y_vals = [lm.z for i, lm in enumerate(landmarks) if i not in range(1,10) and i not in range(17,22)]

    ax.clear()
    ax.scatter(x_vals, y_vals, z_vals, c='r', marker='o')

    # Set axis limits and labels
    ax.set_xlim(0, 1)
    ax.set_ylim(0, 3)
    ax.set_zlim(0, 1)

    ax.set_xlabel('X Label')
    ax.set_ylabel('Y Label')
    ax.set_zlabel('Z Label')
    
    ax.view_init(elev=90, azim=180)  # Adjusted view angle

def main(argv):
    pipeline = Pipeline()
    config = Config()
    parser = argparse.ArgumentParser()
    parser.add_argument("-m", "--mode", help="align mode, HW=hardware mode,SW=software mode,NONE=disable align",
                        type=str, default='HW')
    parser.add_argument("-s", "--enable_sync", help="enable sync", type=bool, default=True)
    args = parser.parse_args(argv)

    enable_sync = args.enable_sync
    temporal_filter = TemporalFilter(alpha=0.5)  # Modify alpha based on desired smoothness

    # Initialize MediaPipe Pose solution
    mp_pose = mp.solutions.pose
    pose = mp_pose.Pose()

    try:
        profile_list = pipeline.get_stream_profile_list(OBSensorType.COLOR_SENSOR)
        color_profile = profile_list.get_video_stream_profile(1280, 720, OBFormat.MJPG, 15)
        config.enable_stream(color_profile)

        profile_list = pipeline.get_stream_profile_list(OBSensorType.DEPTH_SENSOR)
        depth_profile = profile_list.get_default_video_stream_profile()
        config.enable_stream(depth_profile)
    except Exception as e:
        print(e)
        return

    if enable_sync:
        try:
            pipeline.enable_frame_sync()
        except Exception as e:
            print(e)

    try:
        pipeline.start(config)
    except Exception as e:
        print(e)
        pipeline.stop()
        return

    align_filter = AlignFilter(align_to_stream=OBStreamType.COLOR_STREAM)

    # Set up matplotlib for side-by-side video stream and 3D plot
    fig = plt.figure(figsize=(12, 6))
    ax_image = fig.add_subplot(121)
    ax_3d = fig.add_subplot(122, projection='3d')

    # Create a slider for the alpha value of the temporal filter
    alpha_ax = plt.axes([0.1, 0.02, 0.8, 0.03])  # Position [left, bottom, width, height]
    alpha_slider = Slider(alpha_ax, 'Alpha', 0.0, 1.0, valinit=temporal_filter.alpha)

    plt.ion()  # Turn on interactive mode for live updates

    def update_alpha(val):
        temporal_filter.alpha = val

    alpha_slider.on_changed(update_alpha)  # Update alpha on slider change

    while True:
        try:
            frames = pipeline.wait_for_frames(100)
            if not frames:
                continue
            color_frame = frames.get_color_frame()
            depth_frame = frames.get_depth_frame()
            if not color_frame or not depth_frame:
                continue

            frames = align_filter.process(frames)
            color_frame = frames.get_color_frame()
            depth_frame = frames.get_depth_frame()

            color_image = frame_to_bgr_image(color_frame)
            if color_image is None:
                print("Failed to convert frame to image")
                continue

            # Process frame with MediaPipe Pose
            result = pose.process(color_image)

            # Draw pose landmarks on the frame
            if result.pose_landmarks:
                mp.solutions.drawing_utils.draw_landmarks(
                    color_image, result.pose_landmarks, mp_pose.POSE_CONNECTIONS
                )

            depth_data = np.frombuffer(depth_frame.get_data(), dtype=np.uint16).reshape(
                (depth_frame.get_height(), depth_frame.get_width())
            )
            depth_data = depth_data.astype(np.float32) * depth_frame.get_depth_scale()
            depth_data = temporal_filter.process(depth_data)  # Apply temporal filtering

            # If landmarks are detected, adjust the z-coordinate based on depth and plot in 3D
            if result.pose_landmarks:
                landmarks = result.pose_landmarks.landmark
                for lm in landmarks:
                    if 0 <= lm.x <= 1 and 0 <= lm.y <= 1:
                        # Convert landmark x and y to pixel coordinates
                        x_pixel = int(lm.x * color_image.shape[1])
                        y_pixel = int(lm.y * color_image.shape[0])

                        # Get the corresponding depth value
                        depth_value = depth_data[y_pixel, x_pixel]

                        # Replace the z-coordinate with the depth value (scaled for proper units if needed)
                        lm.z = depth_value / 1000.0  # Assuming depth is in millimeters, convert to meters

                # Plot 3D landmarks
                plot_3d_landmarks(landmarks, ax_3d)

            # Display the video frame next to the 3D plot
            ax_image.clear()
            ax_image.imshow(cv2.cvtColor(color_image, cv2.COLOR_BGR2RGB))
            ax_image.set_title("Video Stream")
            ax_image.axis('off')

            # Update the figure
            plt.pause(0.01)  # Pause to allow for the figure to update

            if cv2.waitKey(1) in [ord('q'), ESC_KEY]:
                break
        except KeyboardInterrupt:
            break

    pipeline.stop()
    plt.ioff()  # Turn off interactive mode
    plt.show()

if __name__ == "__main__":
    main(sys.argv[1:])
