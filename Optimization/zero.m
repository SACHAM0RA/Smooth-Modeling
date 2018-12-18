function out = zero( x )
%ZERO Summary of this function goes here
%   Detailed explanation goes here

out = step(x+0.005)*step(0.005-x);
    
end

