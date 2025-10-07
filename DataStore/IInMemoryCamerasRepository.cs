using EIR_9209_2.Models;

/// <summary>
/// Defines methods for managing camera data in memory, including geo markers and camera information.
/// </summary>
public interface IInMemoryCamerasRepository
{
    /// <summary>
    /// Retrieves a camera object by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the camera.</param>
    /// <returns>The camera object associated with the specified id.</returns>
    object Get(string id);

    /// <summary>
    /// Adds a new camera geo marker to the repository.
    /// </summary>
    /// <param name="camera">The <see cref="CameraGeoMarker"/> to add.</param>
    /// <returns>The added <see cref="CameraGeoMarker"/>.</returns>
    Task<CameraGeoMarker> Add(CameraGeoMarker camera);

    /// <summary>
    /// Updates an existing camera geo marker in the repository.
    /// </summary>
    /// <param name="camera">The <see cref="CameraGeoMarker"/> to update.</param>
    Task Update(CameraGeoMarker camera);

    /// <summary>
    /// Deletes a camera geo marker by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the camera geo marker to delete.</param>
    /// <returns>The deleted <see cref="CameraGeoMarker"/>.</returns>
    Task<CameraGeoMarker> Delete(string id);

    /// <summary>
    /// Retrieves all camera geo markers from the repository.
    /// </summary>
    /// <returns>A list of all <see cref="CameraGeoMarker"/> objects.</returns>
    List<CameraGeoMarker> GetAll();

    /// <summary>
    /// Adds camera information to the repository.
    /// </summary>
    /// <param name="camera">The <see cref="Cameras"/> object to add.</param>
    /// <returns>The added <see cref="Cameras"/> object.</returns>
    Task<Cameras> AddCameraInfo(Cameras camera);

    /// <summary>
    /// Updates camera information in the repository.
    /// </summary>
    /// <param name="camera">The <see cref="Cameras"/> object to update.</param>
    /// <returns>The updated <see cref="Cameras"/> object.</returns>
    Task<Cameras> UpdateCameraInfo(Cameras camera);

    /// <summary>
    /// Deletes camera information by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the camera to delete.</param>
    /// <returns>The deleted <see cref="Cameras"/> object.</returns>
    Task<Cameras> DeleteCameraInfo(string id);

    /// <summary>
    /// Retrieves all camera information from the repository.
    /// </summary>
    /// <returns>A list of all <see cref="Cameras"/> objects.</returns>
    List<Cameras> GetCameraListAll();

    /// <summary>
    /// Retrieves camera information by IP address.
    /// </summary>
    /// <param name="Ip">The IP address of the camera.</param>
    /// <returns>The <see cref="Cameras"/> object associated with the specified IP address.</returns>
    Task<Cameras> GetCameraListByIp(string Ip);

    /// <summary>
    /// Loads a list of camera information into the repository.
    /// </summary>
    /// <param name="cameraList">The list of <see cref="Cameras"/> objects to load.</param>
    Task LoadCameraData(List<Cameras> cameraList);

    /// <summary>
    /// Loads camera still images into the repository.
    /// </summary>
    /// <param name="result">The byte array containing the image data.</param>
    /// <param name="id">The unique identifier of the camera.</param>
    Task LoadCameraStills(byte[] result, string id);

    /// <summary>
    /// Resets the cameras list in the repository.
    /// </summary>
    /// <returns><c>true</c> if the reset was successful; otherwise, <c>false</c>.</returns>
    Task<bool> ResetCamerasList();

    /// <summary>
    /// Sets up the cameras list in the repository.
    /// </summary>
    /// <returns><c>true</c> if the setup was successful; otherwise, <c>false</c>.</returns>
    Task<bool> SetupCamerasList();
}