function [ out ] = gaussian( x )


c=0.01;
out = exp(-0.5*(x*c)^2);
end

