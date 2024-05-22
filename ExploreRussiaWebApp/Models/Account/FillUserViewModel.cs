using System.ComponentModel.DataAnnotations;

namespace ExploreRussiaWebApp.Models.Account
{
    public class FillUserViewModel
    {
        [Required(ErrorMessage = "Поле 'Имя' обязательно для заполнения")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Поле 'Фамилия' обязательно для заполнения")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        public string Patronymic { get; set; }

        [Required(ErrorMessage = "Поле 'Телефон' обязательно для заполнения")]
        [RegularExpression(@"^\+[1-9]\d{8,14}$", ErrorMessage = "Введите корректный номер телефона в формате +1234567890")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }
    }
}
