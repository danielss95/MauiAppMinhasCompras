using MauiAppMinhasCompras.Models;

namespace MauiAppMinhasCompras.Views;

public partial class Relatorio : ContentPage
{
    bool pronto = false;

    public Relatorio()
    {
        InitializeComponent();

        dtp_fim.Date = DateTime.Today;
        dtp_inicio.Date = DateTime.Today.AddDays(-30);

        pronto = true;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await GerarRelatorio();
    }

    private async void Filtro_DateSelected(object sender, DateChangedEventArgs e)
    {
        if (!pronto) return;
        await GerarRelatorio();
    }

    private async Task GerarRelatorio()
    {
        try
        {
            DateTime inicio = dtp_inicio.Date.Date;
            DateTime fim = dtp_fim.Date.Date;

            if (inicio > fim)
            {
                await DisplayAlert("Atenção",
                    "A data inicial não pode ser maior que a data final.", "OK");
                return;
            }

            List<Produto> todos = await App.Db.GetAll();

            List<Produto> filtrados = todos
                .Where(p => p.DataCadastro.Date >= inicio && p.DataCadastro.Date <= fim)
                .OrderByDescending(p => p.DataCadastro)
                .ToList();

            List<ResumoCategoria> resumo = filtrados
                .GroupBy(p => string.IsNullOrEmpty(p.Categoria) ? "Sem categoria" : p.Categoria)
                .Select(g => new ResumoCategoria
                {
                    Categoria = g.Key,
                    Total = g.Sum(p => p.Total)
                })
                .OrderByDescending(r => r.Total)
                .ToList();

            BindableLayout.SetItemsSource(stk_categorias, resumo);
            cv_produtos.ItemsSource = filtrados;
            lbl_total.Text = $"Total do período: {filtrados.Sum(p => p.Total):C}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }
}