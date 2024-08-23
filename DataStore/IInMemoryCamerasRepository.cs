using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryCamerasRepository
{
    object Get(string id);
    Task<CameraMarker> Add(CameraMarker camera);
    Task Update(CameraMarker camera);
    Task<CameraMarker> Delete(string id);
    List<CameraMarker> GetAll();
    Task<Cameras> AddCameraInfo(Cameras camera);
    Task<Cameras> UpdateCameraInfo(Cameras camera);
    Task<Cameras> DeleteCameraInfo(string id);
    List<Cameras> GetCameraListAll();
    Task<Cameras> GetCameraListByIp(string Ip);
    void LoadCameraData(List<Cameras> cameraList);
    Task LoadCameraStills(byte[] result, string id);
}