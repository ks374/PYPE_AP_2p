function [xxx] = p2m_dir_windows(filename, directory)


fnames = dir(filename)
for(i=1:size(fnames,1))
    xxx{i} = sprintf('%s%s%s',directory,filesep,fnames(i).name);
end;
