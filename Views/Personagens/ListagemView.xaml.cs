using AppRpgEtec.Models;
using AppRpgEtec.ViewModels.Personagens;

namespace AppRpgEtec.Views.Personagens;

public partial class ListagemView : ContentPage
{
	ListagemPersonagemViewModel viewModel;
    private Personagem personagemSelecionado;

    public ListagemView()
	{
		InitializeComponent();

		viewModel = new ListagemPersonagemViewModel();
		BindingContext = viewModel;
		Title = "Personagens - App Rpg Etec";
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		_ = viewModel.ObterPersonagens();
    }

    public async Task ExibirOpcoesAsync(Personagem personagem)
    {
        try
        {
            personagemSelecionado = null;
            string result = string.Empty;

            if (personagem.PontosVida > 0)
            {
                result = await Application.Current.MainPage
                    .DisplayActionSheet("Opções para o personagem " + personagem.Nome,
                    "Cancelar",
                    null,
                    "Editar Personagem",
                    "Restaurar Pontos de Vida",
                    "Zerar Ranking do Personagem",
                    "Remover Personagem");
            }
            else
            {
                result = await Application.Current.MainPage
                    .DisplayActionSheet("Opções para o personagem " + personagem.Nome,
                    "Cancelar",
                    null,
                    "Restaurar Pontos de Vida");
            }

            if (result != null)
                ProcessarOpcaoRespondidaAsync(personagem, result);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage
                .DisplayAlert("Ops...", ex.Message, "Ok");
        }
    }

    private void ProcessarOpcaoRespondidaAsync(Personagem personagem, string result)
    {
        throw new NotImplementedException();
    }
}