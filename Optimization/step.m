function out = step( x )
%STEP Summary of this function goes here
%   Detailed explanation goes here

if(isnan(x) || ~isreal(x))
    out = NaN;
elseif (x==0)
    out = 0.5;
elseif (x>0)
    out = 1;
elseif (x<0)
    out = 0;

end

