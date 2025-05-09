import nidaqmx
from nidaqmx.constants import AcquisitionType
import os
import pandas as pd
import time
import numpy as np

def get_next_filename(folder, prefix="test"):
    """Finds the next available filename like `prefix_XXX.csv`."""
    os.makedirs(folder, exist_ok=True)  # Create folder if missing
    existing_files = [f for f in os.listdir(folder) if f.startswith(prefix) and f.endswith(".csv")]
    max_num = 0
    for f in existing_files:
        try:
            num = int(f.split("_")[-1].split(".")[0])
            max_num = max(max_num, num)
        except ValueError:
            pass
    return f"{folder}/{prefix}_{max_num + 1:03d}.csv"

def read_and_save_data(folder, prefix="test", sample_rate=100, samples_per_file=1000000):
    """Reads PFI6 in small chunks and appends data to CSV files."""
    next_file = get_next_filename(folder, prefix)
    samples_in_current_file = 0  # Track how many samples are in the current CSV
    
    with nidaqmx.Task() as task:
        task.di_channels.add_di_chan("PXI1Slot4/port0/line6")  # PFI6
        task.di_channels.add_di_chan("PXI1Slot4/port0/line7")  # This should be the photodiode channel. Select the correct one
        task.timing.cfg_samp_clk_timing(
            rate=sample_rate,
            source="OnboardClock",  # Note: two inputs have to be on the same card. 
            sample_mode=AcquisitionType.CONTINUOUS
        )
        task.in_stream.input_buf_size = sample_rate * 60
        
        task.start()
        print(f"Logging to: {next_file}")
        with open(next_file,'w') as f:
            f.write("FrameClock,Photodiode\n")
            while True:
                # Read a small chunk (e.g., 10 samples)
                available_samples = task.in_stream.avail_samp_per_chan
                if available_samples == 0:
                    continue
                data = task.read(number_of_samples_per_channel=available_samples)  # Read ALL available
                
                # Append to CSV (or create new file if reaching samples_per_file)
                if samples_in_current_file + available_samples > samples_per_file:
                    f.close()
                    next_file = get_next_filename(folder, prefix)
                    samples_in_current_file = 0
                    f = open(next_file,'w')
                    f.write("FrameClock,Photodiode\n")
                    print(f"Starting new file: {next_file}")
                
                # Write/append data
                csv_lines = [
                    f"{int(data[0][i])},{int(data[1][i])}\n"
                    for i in range(available_samples)
                ]
                f.writelines(csv_lines)
                samples_in_current_file += len(data)
                
                #print(f"Appended {len(data)} samples to {next_file} (Total: {samples_in_current_file})")

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--folder", default="D:\\Chenghang_Work\\PYPE_AP_2p\\Matlab_Signal_sync_lib\\Output\\", help="Folder to save CSV files")
    parser.add_argument("--prefix", default="test", help="Filename prefix (e.g., 'test')")
    args = parser.parse_args()
    
    read_and_save_data(folder=args.folder, prefix=args.prefix)