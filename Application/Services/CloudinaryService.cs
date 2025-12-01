using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Application.Services;

public interface ICloudinaryService
{
    Task<string> UploadFile(string base64Image, string fileName = "file");
    Task<List<string>> UploadFiles(IEnumerable<string> base64Images, IEnumerable<string>? fileNames = null);
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

    public async Task<string> UploadFile(string base64File, string fileName = "file")
    {
        // Nếu base64 có dạng: "data:image/png;base64,iVBORw0KGgoAAAANS..." thì tách phần dữ liệu ra
        var base64Data = base64File.Contains(',') ? base64File.Split(',')[1] : base64File;

        // Convert base64 string to byte[]
        var fileBytes= Convert.FromBase64String(base64Data);

        await using var stream = new MemoryStream(fileBytes);

        var uploadParams = new RawUploadParams 
        {
            File = new FileDescription(fileName, stream),
            Folder = "TicketManagement-files"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception($"Error uploading file: {uploadResult.Error.Message}");

        return uploadResult.SecureUrl.ToString();
    }
    
    public async Task<List<string>> UploadFiles(IEnumerable<string> base64Files, IEnumerable<string>? fileNames = null)
    {
        var base64List = base64Files.ToList();
        var namesList = fileNames?.ToList() ?? Enumerable.Range(0, base64List.Count).Select(i => $"file_{i}").ToList();
        
        var uploadTasks = base64List.Select((file, index) => 
            UploadFile(file, index < namesList.Count ? namesList[index] : $"file_{index}"));
        var results = await Task.WhenAll(uploadTasks);
        return results.ToList();
    }
}