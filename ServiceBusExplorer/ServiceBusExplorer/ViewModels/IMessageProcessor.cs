using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusExplorer.ViewModels
{
    public interface IMessageProcessor
    {
        string Name { get; }

        bool IsRunning { get; }

        Task Task { get; set; }

        CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
