function [] = main()

clc
clear

fid=fopen('./SatFunctions/1V_Polynomial.txt');
tline = fgetl(fid);
tlines = cell(0,1);
while ischar(tline)
    tlines{end+1,1} = tline;
    tline = fgetl(fid);
end
fclose(fid);

OptTol = 1.0000e-20;
stpTol = 0.0000;
funcTol = 0.0000;

nvars=1;
for i=1:30
    
    for j=1:1
    
        strAprxmSatFunc = char(tlines(i))
        strExactSatFunc = strrep( strAprxmSatFunc , 'sigmoid' , 'step' );
        strExactSatFunc = strrep( strExactSatFunc , 'gaussian' , 'zero' );


        funH = str2func(char("@(x)-1*("+strAprxmSatFunc+")"));
        ExfunH = str2func(char("@(x)("+strExactSatFunc+")"));
        stoppingH = @(x,optimValues,state)outfun(ExfunH,x);

        options = optimoptions('fminunc',...
                               'Algorithm','quasi-newton',...
                               'Display' , 'off',...
                               'OptimalityTolerance' , OptTol,...
                               'StepTolerance' , stpTol ,...
                               'FunctionTolerance' , funcTol,...
                               'OutputFcn',stoppingH);

        tic;
        x0=zeros(1,nvars);
        x=x0;
        k=0;
        iter=0;
        while(ExfunH(x)~=1 && iter<10000)
            x0 = (rand(1,nvars)*2-1)*(2^k);
            fx0 = ExfunH(x0);
            while(isnan(fx0) || ~isreal(fx0) || ~isfinite(fx0))
                x0 = (rand(1,nvars)*2-1)*k;
                fx0 = ExfunH(x0);
                k = k+1;
                iter = iter+1;
            end
            iter = iter+1;

            problem = createOptimProblem('fminunc','objective',funH,...
                                         'x0',x0,...
                                         'options',options);

            [x, fval,exitflag,output] = fminunc(problem);
            iter = iter + output.iterations;
            k = k+1;
        end
        
        time(j) = toc;
        results(i,:)=[mean(time) ExfunH(x)];
        
        

        disp("ITERATIONS:                    " + iter)
        disp("SOLUTION:                      " + num2str(x,10)) 
        disp("SATISFYING FUNCTION VALUE:     " + -funH(x))
        disp("OPTIMISATION TIME:             " + time)
        if(ExfunH(x)==1) 
            sat = "true";
        else
            sat = "false"; 
        end
        disp("CONSTRAINT SATISFIED:          " + sat)

        disp('-------------------------------------------------')

    end
end 

fid=fopen('results.txt','w');
fprintf(fid, '%f \t %f \t \n', results');
fclose(fid);

function stop = outfun(func,x) 
    if(func(x)==1)
        stop = true;
    else
        stop = false;
    end
end
end