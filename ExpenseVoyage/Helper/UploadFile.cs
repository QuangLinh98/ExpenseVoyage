namespace ExpenseVoyage.Helper
{
	public class UploadFile
	{
		static readonly string baseFolder = "Uploads";

		public static async Task<string> SaveImage(string folder, IFormFile formFile,
								 IWebHostEnvironment hostEnvironment)
		{
			var imageName = Guid.NewGuid().ToString() + "_" + formFile.FileName;
			var imagePath = Path.Combine(Directory.GetCurrentDirectory(), hostEnvironment.WebRootPath, $"{baseFolder}\\{folder}");
			if (!Directory.Exists(imagePath))
			{
				Directory.CreateDirectory(imagePath);
			}
			var imagePathExact = Path.Combine(imagePath, imageName);
			using (var fileStream = new FileStream(imagePathExact, FileMode.Create))
			{
				await formFile.CopyToAsync(fileStream);
			}
			return Path.Combine(folder, imageName).Replace("\\", "/");
		}
		public static void DeleteImage(IWebHostEnvironment hostEnvironment, string imagePath)
		{
			string deleteImage = "Uploads/" + imagePath;

			var exactingPathImage = Path.Combine(hostEnvironment.WebRootPath, deleteImage);
			if (File.Exists(exactingPathImage))
			{
				File.Delete(exactingPathImage);
			}
		}
	}
}
