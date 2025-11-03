using System.ComponentModel.DataAnnotations;

namespace CollabDoc.Application.Dtos
{
    public class DocumentCreateDto
    {
        [Required(ErrorMessage = "Title không được để trống")]
        [StringLength(100, ErrorMessage = "Title không được vượt quá 100 ký tự")]
        public string Title { get; set; } = string.Empty;


        public string Content { get; set; } = string.Empty;
    }
}
