using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using EIR_9209_2.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController(ILogger<InventoryController> logger, IInMemoryInventoryRepository inventory,IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly ILogger<InventoryController> _logger = logger;
        private readonly IInMemoryInventoryRepository _inventory = inventory;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        // GET: api/<InventoryController>
        [HttpGet]
        [Route("Inventory")]
        public async Task<ActionResult> GetInventoryList()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var inventoryList = await _inventory.GetInventoryList();

                return Ok(inventoryList);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // GET: api/<InventoryController>
        [HttpGet]
        [Route("InventoryCategory")]
        public async Task<ActionResult> GetInventoryCategoryList()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var inventoryCategoryList = await _inventory.GetInventoryCategoryList();

                return Ok(inventoryCategoryList);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // GET: api/<InventoryController>
        [HttpGet]
        [Route("InventoryTracking")]
        public async Task<ActionResult> GetInventoryTrackingList()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var inventoryCategoryList = await _inventory.GetInventoryTrackingList();

                return Ok(inventoryCategoryList);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        // POST api/<InventoryController>
        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult> PostAddInventory([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //convert the JObject to a Connection object
                Inventory inventoryItem = value.ToObject<Inventory>();
                //add the connection id
                inventoryItem.Id = Guid.NewGuid().ToString();
                inventoryItem.CreatedDate = DateTime.Now;
                //add to the connection repository
                var addInventoryItem = await _inventory.Add(inventoryItem);
                if (addInventoryItem != null)
                {
                    return Ok(addInventoryItem);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // POST api/<InventoryController>
        [HttpPost]
        [Route("AddCategory")]
        public async Task<ActionResult> PostAddInventoryCategory([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //convert the JObject to a Connection object
                InventoryCategory inventoryCategoryItem = value.ToObject<InventoryCategory>();
                //add the connection id
                inventoryCategoryItem.Id = Guid.NewGuid().ToString();
                inventoryCategoryItem.CreatedDate = DateTime.Now;
                //add to the connection repository
                var addInventoryCategoryItem = await _inventory.AddCategory(inventoryCategoryItem);
                if (addInventoryCategoryItem != null)
                {
                    return Ok(addInventoryCategoryItem);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // POST api/<InventoryController>
        [HttpPost]
        [Route("AddTracking")]
        public async Task<ActionResult> PostAddInventoryTracking([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //convert the JObject to a Connection object
                InventoryTracking inventoryTrackingItem = value.ToObject<InventoryTracking>();
                //add the connection id
                inventoryTrackingItem.Id = Guid.NewGuid().ToString();
                inventoryTrackingItem.CreatedDate = DateTime.Now;
                //add to the connection repository
                var addInventoryTrackingItem = await _inventory.AddTracking(inventoryTrackingItem);
                if (addInventoryTrackingItem != null)
                {
                    return Ok(addInventoryTrackingItem);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // PUT api/<InventoryController>/5
        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult> PutInventory([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var inventoryItem = value.ToObject<Inventory>();
                var inventoryItemToUpdate = await _inventory.Update(inventoryItem);
                if (inventoryItemToUpdate !=null)
                {
                    await _hubContext.Clients.Group("Inventory").SendAsync("UpdateInventory", inventoryItem);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("UpdateCategory")]
        public async Task<ActionResult> PutInventoryCategory([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var categoryItem = value.ToObject<InventoryCategory>();
                var categoryItemToUpdate = await _inventory.UpdateCategory(categoryItem);
                if (categoryItemToUpdate != null)
                {
                    await _hubContext.Clients.Group("Inventory").SendAsync("UpdateInventoryCategory", categoryItemToUpdate);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("UpdateTracking")]
        public async Task<ActionResult> PutInventoryTracking([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var trackingItem = value.ToObject<InventoryTracking>();
                var trackingItemToUpdate = await _inventory.UpdateTracking(trackingItem);
                if (trackingItemToUpdate != null)
                {
                    await _hubContext.Clients.Group("Inventory").SendAsync("UpdateInventoryTracking", trackingItemToUpdate);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // DELETE api/<InventoryController>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<ActionResult> DeleteInventory(string id)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var inventoryDelete = await _inventory.Delete(id);
                if (inventoryDelete != null)
                {
                    await _hubContext.Clients.Group("Inventory").SendAsync("DeleteInventory", id);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteCategory")]
        public async Task<ActionResult> DeleteInventoryCategory(string id)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var inventoryCategorDelete = await _inventory.DeleteCategory(id);
                if (inventoryCategorDelete != null)
                {
                    await _hubContext.Clients.Group("Inventory").SendAsync("DeleteInventoryCategory", id);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteTracking")]
        public async Task<ActionResult> DeleteInventoryTracking(string id)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var inventoryTrackingDelete = await _inventory.DeleteTracking(id);
                if (inventoryTrackingDelete != null)
                {
                    await _hubContext.Clients.Group("Inventory").SendAsync("DeleteInventoryTracking", id);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}
