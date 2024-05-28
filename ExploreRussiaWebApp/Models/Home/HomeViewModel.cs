using ExploreRussia.Domain.Models;

namespace ExploreRussiaWebApp.Models.Home
{
    public class HomeViewModel
    {
        public List<Trip> Trip { get; set; }
        public List<Guide> Guide { get; set; }

        public HomeViewModel(List<Guide> guide, List<Trip> trip)
        {
            Trip = trip;
            Guide = guide;
        }
    }
}
