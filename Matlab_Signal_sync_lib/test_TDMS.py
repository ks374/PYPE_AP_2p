import nidaqmx
from nidaqmx.constants import AcquisitionType, LoggingMode
import numpy as np

# Configuration
sample_rate = 1000  # Hz
samples_per_read = 1000
output_file = "digital_data.tdms"

with nidaqmx.Task() as task:
    # Add two digital channels (PFI6 and PFI7)
    task.di_channels.add_di_chan("PXI1Slot4/port0/line6")  # Replace with your device
    task.di_channels.add_di_chan("PXI1Slot4/port0/line7")

    # Configure timing (shared clock)
    task.timing.cfg_samp_clk_timing(
        rate=sample_rate,
        source="OnboardClock",
        sample_mode=AcquisitionType.CONTINUOUS
    )

    # Configure TDMS logging (automatic timestamps)
    task.in_stream.configure_logging(
        output_file,
        logging_mode=LoggingMode.LOG_AND_READ,
        group_name="Digital_Data"  # Custom group name in TDMS
    )

    # Start acquisition
    task.start()
    print(f"Logging to {output_file}... Press Ctrl+C to stop.")

    try:
        while True:
            # Read available samples (no stream_readers needed)
            data = task.read(number_of_samples_per_channel=samples_per_read)
            
            # Data is auto-saved to TDMS; no manual write required
            print(f"Read {len(data[0])} samples (Ch1: {data[0][:3]}..., Ch2: {data[1][:3]}...)")

    except KeyboardInterrupt:
        print("Stopping...")