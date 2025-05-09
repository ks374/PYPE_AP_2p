% Create a DAQ session
s = daq.createSession('ni');
s.Rate = 10000; % Sample rate (match ScanImage if possible)

% Add analog input (e.g., AI0)
s.addAnalogInputChannel('Dev1', 'ai0', 'Voltage');

% Add digital input for frame trigger (e.g., PFI0)
s.addDigitalChannel('Dev1', 'port0/line0', 'InputOnly');

% Start acquisition
[data, timestamps] = s.startForeground();

% Extract frame triggers (assuming TTL pulses)
frameStarts = find(diff(data(:,2)) > 0.5); % Column 2 = digital input
frameTimes = timestamps(frameStarts);

% Now align analog data (data(:,1)) with frameTimes