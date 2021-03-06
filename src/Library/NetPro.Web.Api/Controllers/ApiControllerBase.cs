﻿using NetPro.Core;
using NetPro.Core.Consts;
using NetPro.Core.Infrastructure;
using NetPro.Web.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Serilog;
using System;
using NetPro.Web.Core.Middlewares;
using NetPro.Web.Core.Helpers;

namespace NetPro.Web.Api.Controllers
{
	/// <summary>
	/// api 基类
	/// </summary>
	[ApiController]
	//[Route("api/v1/[controller]")]
	//[Authorize]
	public abstract class ApiControllerBase : ControllerBase
	{
		#region api返回结果封装
		/// <summary>
		/// 错误返回
		/// </summary>
		/// <param name="msg">错误消息</param>
		/// <param name="errorCode">http错误码</param>
		/// <returns></returns>
		protected IActionResult ToFailResult(string msg, int errorCode = 100)
		{
			var resultModel = new ApiResultModel()
			{
				ErrorCode = errorCode,
				Msg = msg
			};
			var result = new ObjectResult(resultModel);
			result.StatusCode = errorCode;
			return result;
			//return BadRequest(result);
			//return new JsonResult(result);
		}

		//protected virtual IActionResult ToResult(string msg, int errorCode)
		//{
		//	var result = new ApiResultModel()
		//	{
		//		ErrorCode = errorCode,
		//		Msg = msg
		//	};
		//	return new JsonResult(result);
		//}

		/// <summary>
		/// 成功返回结果.body为空
		/// </summary>
		/// <param name="msg">成功提示消息</param>
		/// <returns></returns>
		protected virtual IActionResult ToSuccessResult(string msg = "")
		{
			var result = new ApiResultModel()
			{
				ErrorCode = 0,
				Msg = msg
			};
			return new JsonResult(result);
		}

		/// <summary>
		/// 成功返回结果.带body数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="body"></param>
		/// <param name="msg"></param>
		/// <returns></returns>
		protected virtual IActionResult ToSuccessResult<T>(T body, string msg = "")
		{
			var result = new ApiResultModel<T>()
			{
				Body = body,
				ErrorCode = 0,
				Msg = msg
			};
			return new JsonResult(result);
		}
		#endregion

		/// <summary>
		/// 获取发起请求的系统平台
		/// </summary>
		/// <returns></returns>
		[NonAction]
		protected EnumAppPlatform GetPattern()
		{
			try
			{
				var result = Request.Headers["FromApp"].FirstOrDefault();

				var app = result;

				if (string.IsNullOrEmpty(app))
					return EnumAppPlatform.None;

				var pattern = EnumAppPlatform.None;

				switch (app.ToLower())
				{
					case "android":
						pattern = EnumAppPlatform.Android;
						break;
					case "ios":
						pattern = EnumAppPlatform.IOS;
						break;
					case "windows":
						pattern = EnumAppPlatform.Windows;
						break;
					case "Web":
						pattern = EnumAppPlatform.Web;
						break;
				}
				return pattern;
			}
			catch (Exception ex)
			{
				Serilog.Log.Error("BaseApiController.GetPattern", ex);
			}
			return EnumAppPlatform.None;
		}

		/// <summary>
		/// 获取客户端访问IP
		/// </summary>
		/// <returns></returns>
		[NonAction]
		protected string GetIP()
		{
			return EngineContext.Current.Resolve<IWebHelper>().GetCurrentIpAddress();
		}
	}
}



