using Microsoft.AspNetCore.Razor.TagHelpers;
using TechShare.Enums;

namespace TechShare.TagHelpers
{
    [HtmlTargetElement("rental-status")]
    public class RentalStatusTagHelper : TagHelper
    {
        public RentalStatus Status { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            string statusText = Status switch
            {
                RentalStatus.ChoDuyet => "Chờ duyệt",
                RentalStatus.DaDuyet => "Đã duyệt",
                RentalStatus.DangGiao => "Đang giao máy",
                RentalStatus.DangThue => "Đang Thuê",
                RentalStatus.ChoTra => "Chờ kiểm tra trả",
                RentalStatus.HoanTat => "Hoàn Tất",
                RentalStatus.DaHuy => "Đã Hủy",
                RentalStatus.TranhChap => "Chờ xử lý sự cố",
                _ => "Không xác định"
            };

            string badgeClass = Status switch
            {
                RentalStatus.ChoDuyet => "bg-warning text-dark",
                RentalStatus.DaDuyet => "bg-info text-dark",
                RentalStatus.DangGiao => "bg-info text-dark",
                RentalStatus.DangThue => "bg-primary",
                RentalStatus.ChoTra => "bg-warning text-dark",
                RentalStatus.HoanTat => "bg-success",
                RentalStatus.DaHuy => "bg-secondary",
                RentalStatus.TranhChap => "bg-danger",
                _ => "bg-secondary"
            };

            var existingClass = context.AllAttributes["class"]?.Value.ToString();
            var mergedClass = $"badge {badgeClass} {existingClass}".Trim();
            
            output.Attributes.SetAttribute("class", mergedClass);
            output.Content.SetContent(statusText);
        }
    }
}
