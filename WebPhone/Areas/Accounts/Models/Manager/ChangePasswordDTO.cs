﻿using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Accounts.Models.Manager
{
    public class ChangePasswordDTO
    {
        [Display(Name = "Mật khẩu cũ")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = null!;

        [Display(Name = "Mật khẩu mới")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự")]
        public string NewPassword { get; set; } = null!;

        [Display(Name = "Nhập lại mật khẩu mới")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không chính xác")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
