using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Application.Services;

public interface ICloudinaryService
{
    Task<string> UploadFile(string base64Image);
    Task<List<string>> UploadFiles(IEnumerable<string> base64Images);
}

public class CloudinaryService: ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    
    public CloudinaryService()
    {
        var acc = new Account
        {
            Cloud = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUDNAME"),
            ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_APIKEY"),
            ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_APISECRET")
        };
        
        _cloudinary = new Cloudinary(acc);
    }

    public async Task<string> UploadFile(string base64File)
    {
        // Nếu base64 có dạng: "data:image/png;base64,iVBORw0KGgoAAAANS..." thì tách phần dữ liệu ra
        var base64Data = base64File.Contains(',') ? base64File.Split(',')[1] : base64File;

        // Convert base64 string to byte[]
        var fileBytes= Convert.FromBase64String(base64Data);

        await using var stream = new MemoryStream(fileBytes);

        var uploadParams = new RawUploadParams 
        {
            File = new FileDescription("Base64File.png", stream),
            Folder = "TicketManagement-files"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception($"Error uploading file: {uploadResult.Error.Message}");

        return uploadResult.SecureUrl.ToString();
    }
    
    public async Task<List<string>> UploadFiles(IEnumerable<string> base64Files)
    {
        var uploadTasks = base64Files.Select(UploadFile);
        var results = await Task.WhenAll(uploadTasks);
        return results.ToList();
    }
}