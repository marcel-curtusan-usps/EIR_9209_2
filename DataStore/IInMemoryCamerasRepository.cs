using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryCamerasRepository
{
    object Get(string id);
    Task Add(CameraMarker camera);
    List<CameraMarker> GetAll();
    List<string> GetCameraListAll();
    Task LoadCameraData(JToken result);
    Task LoadPictureData(byte[] image, string id, bool picload);
}