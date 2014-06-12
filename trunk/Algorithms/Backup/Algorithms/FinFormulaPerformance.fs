namespace Algorithm
    module List =
       let rec skip n xs = 
          match (n, xs) with
          | 0, _ -> xs
          | _, [] -> []
          | n, _::xs -> skip (n-1) xs

    module DecisionCalculator123456789=

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
                //| _ when i < n-2 -> ema.[i] <- 0m
                | _ when i > n-2 -> ema.[i] <- alpha * p + (1m - alpha) * ema.[i-1]
                | _              -> ignore i)
            // set initial ema value (sma) to 0
            ema.[n-2] <- 0m
            Array.toList ema

        let emaArray (n:int, prices:List<decimal>)=
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
            for i in n-1 .. List.length prices - 1 do
                let c = prices.[i]
                let prev = ema.[i-1]
                ema.[i] <- alpha * c + (1m - alpha) * prev
            // set initial ema value (sma) to 0
            ema.[n-2] <- 0m
            ema

        let emaSeq (n:int, prices:List<decimal>)=
            let alpha = nToAlpha n
            // t-1: calculate average of first n-1 elements as initial value for the ema
            let p = List.toArray prices
            let tm1 =
                prices
                |> Seq.take (n-1)
                |> Seq.average
            let prev = ref 0m
            // create output array
            let ema = seq{for i in 0 .. n - 2 do yield 0m
                          for i in n-1 .. List.length prices - 1 do 
                            yield alpha * prices.[i] + (1m - alpha) * !prev
                            prev := (alpha * prices.[i] + (1m - alpha) * !prev)
                          }
            ema

        let startCalculation (prices : System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, signals : System.Collections.Generic.List<int>)= 
            let n = 20
            let skip = if signals.Count-n-1 > 0 then signals.Count+1-n else 0

            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> List.ofSeq
                |> Seq.skip skip
                |> Seq.toList

            // Self made
            let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            let ema1 = ema (n, cPrices)
            for i in ema1 do
                i |> ignore
            stopWatch.Stop()
            printfn "List: %f" (stopWatch.Elapsed.TotalMilliseconds / 1000.0)

            let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            let ema2 = emaArray (n, cPrices)
            for i in ema2 do
                i |> ignore
            stopWatch.Stop()
            printfn "Array: %f" (stopWatch.Elapsed.TotalMilliseconds / 1000.0)

            let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            let ema3 = emaSeq (n, cPrices)
            for i in ema3 do
                i |> ignore
            stopWatch.Stop()
            printfn "Sequence: %f" (stopWatch.Elapsed.TotalMilliseconds / 1000.0)

            new System.Collections.Generic.List<int>()