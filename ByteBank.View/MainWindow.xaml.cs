using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Utils;
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
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();

            TrocarBotao(BtnProcessar, BtnCancelar);

            var contas = r_Repositorio.GetContaClientes();
            PgsProgresso.Maximum = contas.Count();

            AtualizarMensagemProcessamento(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;
            var reportadorDeProgresso = new Progress<string>(str => PgsProgresso.Value++);
            try
            {
                var resultado = await ConsolidarContas(contas, reportadorDeProgresso, _cts.Token);
                var fim = DateTime.Now;
                AtualizarMensagemProcessamento(resultado, fim - inicio);
            }
            catch (OperationCanceledException)
            {
                TxtTempo.Text = "Processamento cancelado pelo usuário";
            }
            finally
            {
                TrocarBotao(BtnCancelar, BtnProcessar);
            }
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> reportadorDeProgresso, CancellationToken ct)
        {
            var tarefas = contas.Select(conta =>
                Task.Factory.StartNew(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    var resultadoConsolidacao = r_Servico.ConsolidarMovimentacao(conta);
                    reportadorDeProgresso.Report(resultadoConsolidacao);

                    ct.ThrowIfCancellationRequested();

                    return resultadoConsolidacao;
                }, ct));

            return await Task.WhenAll(tarefas);
        }

        private void AtualizarMensagemProcessamento(IEnumerable<string> result, TimeSpan elapsedTime)
        {
            var mensagem = "Processando...";

            if (elapsedTime != TimeSpan.Zero)
                mensagem = $"Processamento de {result.Count()} clientes concluído em { elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            TrocarBotao(BtnCancelar, BtnProcessar);
        }

        public void TrocarBotao(Button atual, Button substituto)
        {
            atual.Visibility = Visibility.Collapsed;
            substituto.Visibility = Visibility.Visible;
        }
    }
}
