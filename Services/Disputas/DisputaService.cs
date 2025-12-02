using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppRpgEtec.Models;

namespace AppRpgEtec.Services.Disputas
{
    public class DisputaService : Request
    {
        private readonly Request _request;
        private string _token;

        //xyz --> site da sua API 
        private const string _apiUrlBase = "https://isabelinha-rpg.azurewebsites.net/Disputas";

        public DisputaService(string token)
        {
            _request = new Request();
            _token = token;
        }

        public async Task<Disputa> PostDisputaComArmaAsync(Disputa d)
        {
            string urlComplementar = "/Arma";
            return await _request.PostAsync(_apiUrlBase + urlComplementar, d, _token);
        }

        public async Task<Disputa> PostDisputaComHabilidadesAsync(Disputa d)
        {
            string urlComplementar = "/Habilidade";
            return await _request.PostAsync(_apiUrlBase + urlComplementar, d, _token);
        }

        public async Task<Disputa> PostDisputaGeralAsync(Disputa d)
        {
            string urlComplementar = "/DisputaEmGrupo";
            return await _request.PostAsync(_apiUrlBase + urlComplementar, d, _token);
        }

        internal async Task<Disputa> DisputaComArmaAsync(Disputa disputaPersonagens)
        {
            throw new NotImplementedException();
        }

        internal async Task<Disputa> DisputaComHabilidadeAsync(Disputa disputaPersonagens)
        {
            throw new NotImplementedException();
        }

        internal async Task<Disputa> DisputaGeralAsync(Disputa disputaPersonagens)
        {
            throw new NotImplementedException();
        }
    }
}
