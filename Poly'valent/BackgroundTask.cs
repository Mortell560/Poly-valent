namespace Poly_valent
{
    public class BackgroundTask
    {
        private Task? _timerTask;
        private readonly PeriodicTimer _timer;
        private readonly CancellationTokenSource _cts= new();

        public BackgroundTask(TimeSpan interval)
        {
            _timer = new(interval);
        }
        public void Start()
        {
            _timerTask = Update();
        }

        private async Task Update()
        {
            try
            {
                while(await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    // task to repeat
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
        
        public async Task StopAsync()
        {
            if (_timerTask is null)
            {
                return;
            }
            _cts.Cancel();
            await _timerTask;
            _cts.Dispose();

            Console.WriteLine("Background tasks were cancelled");
        }
    }
}
