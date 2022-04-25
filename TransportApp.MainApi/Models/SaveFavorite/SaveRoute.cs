using System.Collections.Generic;

namespace TransportApp.MainApi.Models.SaveFavorite
{
    public class SaveRoute : BaseSaveFavorite
    {
        public List<string> SaveRoutes { get; set; }
    }
}
