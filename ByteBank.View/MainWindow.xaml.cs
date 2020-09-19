using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            BtnProcessar.IsEnabled = false;
            var threadPrincipal = TaskScheduler.FromCurrentSynchronizationContext();

            var contas = r_Repositorio.GetContaClientes();
            
            var resultado = new List<string>();

            AtualizarMensagemProcessamento(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            var tarefas = contas.Select(conta =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                });
            }).ToArray();

            Task.WhenAll(tarefas)
                .ContinueWith(task =>
                {
                    var fim = DateTime.Now;
                    AtualizarMensagemProcessamento(resultado, fim - inicio);
                }, threadPrincipal)
                .ContinueWith(task =>
                {
                    BtnProcessar.IsEnabled = true;
                }, threadPrincipal);
        }

        private void AtualizarMensagemProcessamento(List<String> result, TimeSpan elapsedTime)
        {
            var mensagem = "Processando...";

            if (elapsedTime != TimeSpan.Zero)
                mensagem = $"Processamento de {result.Count} clientes concluído em { elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}
