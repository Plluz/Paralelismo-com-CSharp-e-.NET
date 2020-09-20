using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

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
