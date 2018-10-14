using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Authentication.Embedded.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Authentication.Embedded.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly IAuthorizationService _authorizationService;

        public OrdersController(OrderService orderService, IAuthorizationService authorizationService)
        {
            _orderService = orderService;
            _authorizationService = authorizationService;
        }

        [HttpGet("")]
        [Authorize("ViewOrders")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAll();

            var email = User.FindFirst(c => c.Type == JwtClaimTypes.Email)?.Value;
            if (email != null)
            {
                orders = orders.Where(o => o.OrderedBy == email);
            }

            return Ok(orders);
        }

        [HttpGet("{id}", Name = "GetOrderById")]
        [Authorize("ViewOrders")]
        public async Task<ActionResult> GetOrderById(int id)
        {
            var order = await _orderService.Get(id);
            if (order == null)
            {
                return NotFound();
            }

            if ((await _authorizationService.AuthorizeAsync(User, order, SelfOnlyRequirement.Instance)).Succeeded)
            {
                return Ok(order);
            }

            return Forbid();
        }

        [HttpPost("")]
        [Authorize("PlaceOrders")]
        public async Task<IActionResult> NewOrder(OrderRequest orderRequest)
        {
            var email = User.FindFirst(c => c.Type == JwtClaimTypes.Email)?.Value;
            var order = new Order(email, orderRequest);
            var newId = await _orderService.Add(order);

            return CreatedAtRoute("GetOrderById", new { id = newId }, order);
        }

        [HttpDelete("{id}")]
        [Authorize("ManageOrders")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var deleted = await _orderService.Get(id);
            if (deleted == null)
            {
                return NotFound();
            }

            await _orderService.Delete(id);
            return NoContent();
        }
    }
}
