using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq; 
using AuthAPI.Models;
using Microsoft.AspNetCore.Authorization; 

namespace CustomerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserMasterController : ControllerBase
    {
        private readonly AuthDBContext context;
        public UserMasterController(AuthDBContext _context)
        {
            context = _context;
        }
        // GET: api/<CustomerController>
        [HttpGet]
        public IEnumerable<TblUser> Get()
        {
            return context.TblUser.ToList();
        }

        [HttpGet("{id}")]
        public TblUser Get(string id)
        {
            return context.TblUser.FirstOrDefault(o => o.UserId == id);
        }

        [HttpPost("Save")]
        public APIResponse Save([FromBody] TblUser value)
        {
            string result = string.Empty;
            try
            {
                var _emp = context.TblUser.FirstOrDefault(o => o.UserId == value.UserId);
                if (_emp != null)
                {
                    _emp.Role = value.Role;
                    _emp.Email = value.Email;
                    _emp.Name = value.Name;
                    _emp.IsActive = value.IsActive;
                    context.SaveChanges();
                    result = "pass";
                }
                else
                {
                    TblUser tblUser = new TblUser()
                    {
                        Name = value.Name,
                        Email = value.Email,
                        UserId = value.UserId,
                        Role = value.Role,
                       // Password = value.Password,
                        IsActive = true
                    };
                    context.TblUser.Add(tblUser);
                    context.SaveChanges();
                    result = "pass";
                }

            }
            catch (Exception ex)
            {
                result = string.Empty;
            }
            return new APIResponse { keycode = string.Empty, result = result };
        }

        [HttpPost("ActivateUser")]
        public APIResponse ActivateUser([FromBody] TblUser value)
        {
            string result = string.Empty;
            try
            {
                var _emp = context.TblUser.FirstOrDefault(o => o.UserId == value.UserId);
                if (_emp != null)
                {
                    _emp.Role = value.Role;
                    _emp.IsActive = value.IsActive;
                    context.SaveChanges();
                    result = "pass";
                }
            }
            catch (Exception ex)
            {
                result = string.Empty;
            }
            return new APIResponse { keycode = string.Empty, result = result };
        }

        [HttpDelete("{id}")]
        public APIResponse Delete(string id)
        {
            string result = string.Empty;
            var _emp = context.TblUser.FirstOrDefault(o => o.UserId == id);
            if (_emp != null)
            {
                context.TblUser.Remove(_emp);
                context.SaveChanges();
                result = "pass";
            }
            return new APIResponse { keycode = string.Empty, result = result };
        }

    }
}
