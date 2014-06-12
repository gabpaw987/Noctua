namespace Algorithm
    module DecisionCalculator37=
        open System.IO
        (* This method is supposed to read a bar.csv file and convert it to an array of pricestuples*)
        let readFromFile (path:string) =
            let splittedData = (File.ReadAllText(path)).Split([|"\r\n"|], System.StringSplitOptions.RemoveEmptyEntries)
            let test = Array.init (splittedData.Length-2) (fun index -> 
                let a = (splittedData.[index+1]).Split([|','|])
                (System.DateTime.ParseExact(a.[1]+" "+a.[2],"MM/dd/yy HH:mm", new System.Globalization.CultureInfo("en-US")),decimal(a.[3]),decimal(a.[4]),decimal(a.[5]),decimal(a.[6])))
            test

        let alphaToN (a) : int=
            int ((2.0m/a)-1.0m)
            
        let nToAlpha (n:int) : decimal=
            (2.0m / (decimal n + 1.0m))

        let ema (n:int, prices:List<decimal>)=
            let alpha = nToAlpha n
            // t-1: calculate average of first n-1 elements as initial value for the ema

            let tm1 =
                prices
                |> Seq.take (n-1)
                |> Seq.average
            // create output array
            let ema : decimal array = Array.zeroCreate (List.length prices)
            // put initial ema value into output as first t-1 value
            ema.[n-2] <- tm1
            // calculate ema for each price in the list
            prices
            |> List.iteri (fun i p -> 
                match i with
                | _ when i > n-2 -> ema.[i] <- alpha * p + (1m - alpha) * ema.[i-1]
                | _              -> ignore i)
            // set initial ema value (sma) to 0
            ema.[n-2] <- 0m
            ema


        let calculateSignals(indicators:Map<string,decimal[]>)(prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)(signals:System.Collections.Generic.List<int>)=
            //calculate the ema over the given prices
            let valuesOfEMA = ema( 90 , [ for bar in prices -> bar.Item5 ])
            //insert every calculated indicator (maybe delete it before) and return it to the main method
            indicators.Add("EMA",valuesOfEMA)
                .Add("AMA",valuesOfEMA)
                .Add("RSI",valuesOfEMA)
                .Add("Bolliger",valuesOfEMA)
                .Add("ADX",valuesOfEMA)
                .Add("usw",valuesOfEMA)
                .Add("usw",valuesOfEMA)
                .Add("usw",valuesOfEMA)

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            let m = Map.empty.Add("temp",[|0m|])
            let map = calculateSignals(m)(list2D)(signals)

            //"C:/Users/Josefs/Dropbox/Projekte/Diplomprojekt/CAD_1mBar_20110924.csv"
            //"C:/Users/Josefs/Dropbox/Projekte/Diplomprojekt/CAD_1mBar_20110924.csv"
            let data = readFromFile("C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/NKD_1mBar_20110809.csv")


            signals