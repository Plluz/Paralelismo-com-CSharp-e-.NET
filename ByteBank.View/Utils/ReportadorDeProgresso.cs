using System;
using System.Threading;
using System.Threading.Tasks;

namespace ByteBank.View.Utils
{
    class ReportadorDeProgresso<T> : IProgress<T>
    {
        private Action<T> Manipulador { get; }
        private TaskScheduler AgendadorDeTarefas { get; }

        public ReportadorDeProgresso(Action<T> manipulador)
        {
            AgendadorDeTarefas = TaskScheduler.FromCurrentSynchronizationContext();
            Manipulador = manipulador;
        }

        public void Report(T value)
        {
            Task.Factory.StartNew(() =>
                Manipulador(value),
                CancellationToken.None,
                TaskCreationOptions.None,
                AgendadorDeTarefas);
        }
    }
}
