using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AppRpgEtec.Models;
using AppRpgEtec.Services.Personagens;
using AppRpgEtec.Services.PersonagemHabilidades;
using AppRpgEtec.Services.Disputas;
using Microsoft.Maui.Controls;        
using Microsoft.Maui.Storage;
using System.Linq.Expressions;

namespace AppRpgEtec.ViewModels.Disputas
{
    public class DisputaViewModel : BaseViewModel
    {
        
        private readonly PersonagemService _pService;
        private readonly DisputaService _dService;
        private readonly PersonagemHabilidadeService _phService;
        
        private Personagem _personagemSelecionado;
        private Personagem _atacante;
        private Personagem _oponente;
        private PersonagemHabilidade habilidadeSelecionada;
        private string _textoBuscaDigitado = string.Empty;

       
        public ObservableCollection<Personagem> PersonagensEncontrados { get; private set; }
        public ObservableCollection<PersonagemHabilidade> Habilidades { get; private set; }
        public Disputa DisputaPersonagens { get; set; }

        public ICommand PesquisarPersonagensCommand { get; }
        public ICommand DisputaComArmaCommand { get; }
        public ICommand DisputaComHabilidadeCommand { get; }
        public ICommand DisputaGeralCommand { get; }

        public DisputaViewModel()
        {
            string token = Preferences.Get("UsuarioToken", string.Empty);

            _pService = new PersonagemService(token);
            _dService = new DisputaService(token);
            _phService = new PersonagemHabilidadeService(token);

            PersonagensEncontrados = new ObservableCollection<Personagem>();
            Habilidades = new ObservableCollection<PersonagemHabilidade>();

            _atacante = new Personagem();
            _oponente = new Personagem();
            DisputaPersonagens = new Disputa();

            PesquisarPersonagensCommand = new Command<string>(async (q) => await PesquisarPersonagens(q));
            DisputaComArmaCommand = new Command(async () => await ExecutarDisputaArmada());
            DisputaComHabilidadeCommand = new Command(async () => await ExecutarDisputaComHabilidade());
            DisputaGeralCommand = new Command(async () => await ExecutarDisputaGeral());
        }

        public string DescricaoPersonagemAtacante => _atacante?.Nome ?? "---";
        public string DescricaoPersonagemOponente => _oponente?.Nome ?? "---";

        public Personagem PersonagemSelecionado
        {
            get => _personagemSelecionado;
            set
            {
                if (value == null) return;

                _personagemSelecionado = value;
                OnPropertyChanged();
                
                _ = SelecionarPersonagem(value);
            }
        }
        public Personagem Atacante
        {
            get => _atacante;
            set
            {
                _atacante = value;
                OnPropertyChanged(nameof(Atacante));
                OnPropertyChanged(nameof(DescricaoPersonagemAtacante));
            }
        }

        public Personagem Oponente
        {
            get => _oponente;
            set
            {
                _oponente = value;
                OnPropertyChanged(nameof(Oponente));
                OnPropertyChanged(nameof(DescricaoPersonagemOponente));
            }
        }

        public PersonagemHabilidade HabilidadeSelecionada
        {
            get { return habilidadeSelecionada; }
            set
            {
                if (value != null)
                {
                    try
                    {
                        habilidadeSelecionada = value;
                        OnPropertyChanged();
                    }
                    catch (Exception ex)
                    {
                        Application.Current.MainPage.DisplayAlert("Ops", ex.Message, "Ok");
                    }
                }
            }
        }

        public string TextoBuscaDigitado
        {
            get => _textoBuscaDigitado;
            set
            {
                if (_textoBuscaDigitado == value) return;
                _textoBuscaDigitado = value;
                OnPropertyChanged();

                if (!string.IsNullOrWhiteSpace(_textoBuscaDigitado))
                {
                    _ = PesquisarPersonagens(_textoBuscaDigitado);
                }
                else
                {
                    PersonagensEncontrados.Clear();
                }
            }
        }

        public async Task PesquisarPersonagens(string textoPesquisaPersonagem)
        {
            try
            {
                var resultado = await _pService.GetByNomeAproximadoAsync(textoPesquisaPersonagem);

                PersonagensEncontrados.Clear();
                if (resultado != null)
                {
                    foreach (var item in resultado)
                        PersonagensEncontrados.Add(item);
                }
                OnPropertyChanged(nameof(PersonagensEncontrados));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ops", ex.Message + (ex.InnerException != null ? " Detalhes: " + ex.InnerException.Message : ""), "Ok");
            }
        }

        public async Task SelecionarPersonagem(Personagem p)
        {
            try
            {
                if (p == null) return;

                string tipoCombatente = await Application.Current.MainPage.DisplayActionSheet("Atacante ou Oponente?", "Cancelar", null, "Atacante", "Oponente");

                if (tipoCombatente == "Atacante")
                {
                    Atacante = p;
                    await ObterHabiidadeAsync(p.Id);
                }
                else if (tipoCombatente == "Oponente")
                {
                    Oponente = p;
                }

                PersonagensEncontrados.Clear();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ops", ex.Message + (ex.InnerException != null ? " Detalhes: " + ex.InnerException.Message : ""), "Ok");
            }
        }

        public async Task ObterHabiidadeAsync(int personagemId)
        {
            try
            {
                Habilidades.Clear();

                var lista = await _phService.GetPersonagemHabilidadesAsync(personagemId);

                if (lista != null)
                {
                    foreach (var item in lista)
                        Habilidades.Add(item);
                }

                OnPropertyChanged(nameof(Habilidades));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ops", "Não foi possível carregar habilidades: " + ex.Message, "OK");
            }
        }
        private async Task ExecutarDisputaArmada()
        {
            try
            {
                if (_atacante == null || _oponente == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Selecione o Atacante e o Oponente antes de disputar.", "OK");
                    return;
                }

                DisputaPersonagens.AtacanteId = _atacante.Id;
                DisputaPersonagens.OponenteId = _oponente.Id;

               
                var resultado = await _dService.DisputaComArmaAsync(DisputaPersonagens);

                if (resultado != null)
                {
                    DisputaPersonagens = resultado;
                    await Application.Current.MainPage.DisplayAlert("Resultado", DisputaPersonagens.Narracao ?? "Sem narração", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Resultado", "Nenhum resultado retornado.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ops", ex.Message + (ex.InnerException != null ? " Detalhes: " + ex.InnerException.Message : ""), "OK");
            }
        }

        private async Task ExecutarDisputaComHabilidade()
        {
            try
            {
                if (_atacante == null || _oponente == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Selecione o Atacante e o Oponente antes de disputar.", "OK");
                    return;
                }

                if (HabilidadeSelecionada == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Selecione uma habilidade do atacante.", "OK");
                    return;
                }

                DisputaPersonagens.AtacanteId = _atacante.Id;
                DisputaPersonagens.OponenteId = _oponente.Id;
                DisputaPersonagens.HabilidadeId = HabilidadeSelecionada.HabilidadeId;

                var resultado = await _dService.DisputaComHabilidadeAsync(DisputaPersonagens);

                if (resultado != null)
                {
                    DisputaPersonagens = resultado;
                    await Application.Current.MainPage.DisplayAlert("Resultado", DisputaPersonagens.Narracao ?? "Sem narração", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Resultado", "Nenhum resultado retornado.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ops", ex.Message + (ex.InnerException != null ? " Detalhes: " + ex.InnerException.Message : ""), "OK");
            }
        }
        private async Task ExecutarDisputaGeral()
        {
            try
            {
                if (_atacante == null || _oponente == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Selecione o Atacante e o Oponente antes de disputar.", "OK");
                    return;
                }

                DisputaPersonagens.AtacanteId = _atacante.Id;
                DisputaPersonagens.OponenteId = _oponente.Id;

                var resultado = await _dService.DisputaGeralAsync(DisputaPersonagens);

                if (resultado != null)
                {
                    DisputaPersonagens = resultado;
                    await Application.Current.MainPage.DisplayAlert("Resultado", DisputaPersonagens.Narracao ?? "Sem narração", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Resultado", "Nenhum resultado retornado.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ops", ex.Message + (ex.InnerException != null ? " Detalhes: " + ex.InnerException.Message : ""), "OK");
            }
        }
    }
}
