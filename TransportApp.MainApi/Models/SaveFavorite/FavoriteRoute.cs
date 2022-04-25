using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportApp.MainApi.Models.SaveFavorite
{
    public class FavoriteRoute : BaseSaveFavorite
    {
        public List<string> FavoriteRoutes { get; set; }
    }
}
