using MauiAppMinhasCompras.Helpers;
using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;

namespace MauiAppMinhasCompras.Views;

public partial class ListaProduto : ContentPage
{
    ObservableCollection<Produto> lista = new ObservableCollection<Produto>();

    public ListaProduto()
    {
        InitializeComponent();
        lst_produtos.ItemsSource = lista;

        var opcoes = new List<string> { "Todas" };
        opcoes.AddRange(Categorias.Lista);
        pck_filtro.ItemsSource = opcoes;
        pck_filtro.SelectedIndex = 0;
    }

    // Carrega a lista respeitando a busca e a categoria escolhida
    private async Task CarregarLista()
    {
        try
        {
            string q = txt_search.Text ?? "";

            List<Produto> tmp = string.IsNullOrWhiteSpace(q)
                ? await App.Db.GetAll()
                : await App.Db.Search(q);

            string categoria = pck_filtro.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(categoria) && categoria != "Todas")
            {
                tmp = tmp.Where(p => p.Categoria == categoria).ToList();
            }

            lista.Clear();
            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await CarregarLista();
    }

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new Views.NovoProduto());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void txt_search_TextChanged(object sender, TextChangedEventArgs e)
    {
        await CarregarLista();
    }

    private async void pck_filtro_SelectedIndexChanged(object sender, EventArgs e)
    {
        await CarregarLista();
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        double soma = lista.Sum(i => i.Total);
        string msg = $"O total é {soma:C}";
        DisplayAlert("Total dos Produtos", msg, "OK");
    }
    
    private async void ToolbarItem_Clicked_2(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new Views.Relatorio());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            MenuItem selecionado = sender as MenuItem;
            Produto p = selecionado.BindingContext as Produto;

            bool confirm = await DisplayAlert(
                "Tem Certeza?", $"Remover {p.Descricao}?", "Sim", "Não");

            if (confirm)
            {
                await App.Db.Delete(p.Id);
                lista.Remove(p);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            Produto p = e.SelectedItem as Produto;
            if (p == null) return;

            await Navigation.PushAsync(new Views.EditarProduto
            {
                BindingContext = p
            });

            lst_produtos.SelectedItem = null;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void lst_produtos_Refreshing(object sender, EventArgs e)
    {
        try
        {
            await CarregarLista();
        }
        finally
        {
            lst_produtos.IsRefreshing = false;
        }
    }
}