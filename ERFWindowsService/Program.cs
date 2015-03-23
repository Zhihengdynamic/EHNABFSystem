using System;
//using System.Threading.Timer;
using System.Threading;


namespace ERFWinServiceNet5
{
    public class Program
    {
 
        public void Main(string[] args)
        {
            
            int perior = 20*60000; //20*60 seconds
            int dueTime = 2000;
            Console.WriteLine("Start...");
            
            AskService ask = new AskService(dueTime, perior);
            
            ask.timerRef = new System.Threading.Timer(
                new System.Threading.TimerCallback(ask.RunForcasting),
                null,
                Timeout.Infinite,
                Timeout.Infinite);

            ask.start();

            // The main thread does nothing until the timer is disposed.
            while (true)
                Thread.Sleep(1);
            //Console.WriteLine("Timer example done.");

        }

        

        
    }
}
