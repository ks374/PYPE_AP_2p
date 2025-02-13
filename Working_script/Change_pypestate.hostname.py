import pickle

# File paths
inpath = r'D:/Research/Projects/Project_31_Postdoc_Start/PYPE and eye monitoring/PYPE_AP_2p/Working_script/'
input_file = inpath + '/pypestate.eclat_in'  # Path to the input file
output_file = inpath + '/pypestate.eclat'  # Path to the output file

# Load the data from the file
def load_data(filepath):
    """Load serialized data from a file."""
    try:
        with open(filepath, 'rb') as f:
            data = pickle.load(f)
        print("Data loaded successfully:")
        print(data)
        return data
    except Exception as e:
        print(f"Error loading file: {e}")
        return None

# Modify the data
def modify_data(data):
    """Modify the loaded data."""
    if data is None:
        return None

    # Example modifications
    data['eye_yoff'] = -1000000000  # Update vertical offset
    data['eye_tweak'] = '2'  # Update tweak parameter
    data['eye_ygain'] = 0.9  # Update vertical gain
    data['eye_xoff'] = 1500000000  # Update horizontal offset
    data['eye_xgain'] = 0.9  # Update horizontal gain
    
    data['eye_yoff_aux'] = -1000000000  # Update vertical offset
    data['eye_ygain_aux'] = 0.9  # Update vertical gain
    data['eye_xoff_aux'] = 1500000000  # Update horizontal offset
    data['eye_xgain_aux'] = 0.9  # Update horizontal gain

    print("Data modified successfully:")
    print(data)
    return data

# Save the updated data to a file
def save_data(data, filepath):
    """Save serialized data to a file."""
    if data is None:
        print("No data to save.")
        return

    try:
        with open(filepath, 'wb') as f:
            pickle.dump(data, f)
        print(f"Data saved successfully to {filepath}")
    except Exception as e:
        print(f"Error saving file: {e}")

# Main script
if __name__ == '__main__':
    data = load_data(input_file)
    updated_data = modify_data(data)
    save_data(updated_data, output_file)