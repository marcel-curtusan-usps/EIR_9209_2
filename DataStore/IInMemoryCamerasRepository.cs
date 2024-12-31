using EIR_9209_2.Models;

public interface IInMemoryCamerasRepository
{
    object Get(string id);
    Task<CameraGeoMarker> Add(CameraGeoMarker camera);
    Task Update(CameraGeoMarker camera);
    Task<CameraGeoMarker> Delete(string id);
    List<CameraGeoMarker> GetAll();
    Task<Cameras> AddCameraInfo(Cameras camera);
    Task<Cameras> UpdateCameraInfo(Cameras camera);
    Task<Cameras> DeleteCameraInfo(string id);
    List<Cameras> GetCameraListAll();
    Task<Cameras> GetCameraListByIp(string Ip);
    Task LoadCameraData(List<Cameras> cameraList);
    Task LoadCameraStills(byte[] result, string id);
    Task<bool> ResetCamerasList();
    Task<bool> SetupCamerasList();
}