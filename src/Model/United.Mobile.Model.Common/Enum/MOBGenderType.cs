using System.ComponentModel.DataAnnotations;

namespace United.Mobile.Model.Common
{
    public enum MOBGenderType
    {

        [Display(Name = "Undisclosed (U)")]
        U,
        [Display(Name = "Unspecified (X)")]
        X,
        [Display(Name = "Male (M)")]
        M,
        [Display(Name = "Female (F)")]
        F
    }
}
