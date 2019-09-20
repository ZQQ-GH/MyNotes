/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2018-07-09
 * Time: 8:40
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using WBF.Core;
using WBF.Core.Mvc;
using WBF.Service;
using WBF.Utils;

namespace Wesun.SW.M3005.Presentation.F300514
{
	/// <summary>
	/// 主界面视图
	/// 功能：用于数据显示、与用户进行交互
	///       需要操作控制器
	/// </summary>
	public partial class FormMain : FormViewBase
	{
		#region 转换控制器类型
		public new MainController Controller
		{
			get{ return base.Controller as MainController; }
		}
		
		ToolTip m_Hint =new ToolTip();
		string m_strLastHint = "";
		Point m_PtLocation = new Point();
		string m_strLastCell = "";
		#endregion
		
		#region  构造函数
		public FormMain(ControllerBase c):base(c)
		{
			InitializeComponent();
		}
		#endregion

		#region  窗体
		void FormMainLoad(object sender, EventArgs e)
		{
			DBG_List.DataSource =Controller.GetListDataSet();
			bindingCds_Search.SetDataSource(Controller.GetSearchDataSet());
			Btn_ClearClick(null,null);
			Btn_SearchClick(null,null);
		}
		#endregion

		#region  按钮
		void Btn_SearchClick(object sender, EventArgs e)
		{
			Controller.SearchData(CB_ShowMoney.Value);
		}
		
		void Btn_ClearClick(object sender, EventArgs e)
		{
			Controller.SetDefaultSearchValue();
		}
		#endregion

		#region  通用选择
		void LE_UnitNameButtonClick(object sender, EventArgs e)
		{
			Controller.SelectUnitName();
		}
		#endregion

		#region 显示外币
		void CB_ShowMoneyCheckedChanged(object sender, EventArgs e)
		{
			if(CB_ShowMoney.Checked)
			{
				GB_Money.Visible =true;
				GB_origin.Visible =true;
				GB_sale.Visible =true;
				GB_receipt.Visible =true;
				GB_consign.Visible =true;
				GB_surplus.Visible =true;
				GB_supply.Visible =true;
				GB_adjust.Visible =true;
			}
			else
			{
				GB_Money.Visible =false;
				GB_origin.Visible =false;
				GB_sale.Visible =false;
				GB_receipt.Visible =false;
				GB_consign.Visible =false;
				GB_surplus.Visible =false;
				GB_supply.Visible =false;
				GB_adjust.Visible =false;
			}
			Btn_SearchClick(null,null);
		}
		#endregion

		#region  右键按钮
		void Menu_ListOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			MI_Adjust.Enabled=BGV_List.RowCount>0&&BGV_List.FocusedRowHandle>-1;
			MI_ToExcel.Enabled =BGV_List.RowCount>0;
		}
		
		void MI_AdjustClick(object sender, EventArgs e)
		{
			Controller.CallAdjust(BGV_List.GetFocusedDataSourceRowIndex());
		}
		
		void MI_ToExcelClick(object sender, EventArgs e)
		{
			ExcelHelper.ToExcel(DBG_List);
		}
		#endregion
		
		#region  显示明细
		void BGV_ListRowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
		{
			if(e.Clicks>=2&&(e.Column.FieldName.SameText("sale_foreign_amount")||
			                 e.Column.FieldName.SameText("sale_native_amount")||
			                 e.Column.FieldName.SameText("supply_foreign_amount")||
			                 e.Column.FieldName.SameText("supply_native_amount")||
			                 e.Column.FieldName.SameText("receipt_foreign_amount")||
			                 e.Column.FieldName.SameText("receipt_native_amount")||
			                 e.Column.FieldName.SameText("consign_foreign_amount")||
			                 e.Column.FieldName.SameText("consign_native_amount")))
			{
				Controller.MakeDetailCndt(e.Column.FieldName,BGV_List.GetFocusedDataSourceRowIndex());
			}
		}
		#endregion
		
		#region 重绘事件 Added by gyc 2018-11-27 11:00:48
		void BGV_ListCustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
		{
			// Added by gyc 2018-11-27 10:23:46  当该客户有预收、预付金额时，增加颜色标记
			if(e.Column == col_receipt_money)
			{
				if(Convert.ToDouble(BGV_List.GetDataRow(e.RowHandle)["receipt_money"]) >0
				   && e.RowHandle != BGV_List.GetRowHandle(BGV_List.GetFocusedDataSourceRowIndex()))
				{
					e.Appearance.ForeColor=Color.Black;
					e.Appearance.BackColor=Color.SkyBlue;
				}
			}
			else if(e.Column == col_consign_money)
			{
				if(Convert.ToDouble(BGV_List.GetDataRow(e.RowHandle)["consign_money"]) >0
				   && e.RowHandle != BGV_List.GetRowHandle(BGV_List.GetFocusedDataSourceRowIndex()))
				{
					e.Appearance.ForeColor=Color.Black;
					e.Appearance.BackColor=Color.SkyBlue;
				}
			}
			// End of Added
		}
		#endregion
		
		#region 悬浮窗相关 Added by gyc 2018-11-27 11:15:12
		void BGV_ListMouseLeave(object sender, EventArgs e)
		{
			HideHint((sender as GridView).GridControl);
		}
		
		void HideHint(IWin32Window a_winHandle)
		{
			m_strLastHint = "";
			m_Hint.Hide(a_winHandle);
		}
		
		void BGV_ListMouseMove(object sender, MouseEventArgs e)
		{
			m_PtLocation = e.Location;
			DoShowThingGVList();
		}
		
		void DoShowThingGVList()
		{
			if (BGV_List==null || BGV_List.RowCount == 0 || BGV_List.FocusedRowHandle < 0  )
			{
				return;
			}

			Control l_frmOwner = BGV_List.GridControl;
			
			DevExpress.XtraGrid.Views.Grid.Handler.GridHandler l_gh
				= new DevExpress.XtraGrid.Views.Grid.Handler.GridHandler(BGV_List);
			m_PtLocation = Control.MousePosition;
			m_PtLocation = BGV_List.GridControl.PointToClient(m_PtLocation);
			GridHitInfo l_downHitInfo = l_gh.ViewInfo.CalcHitInfo(m_PtLocation);
			if(l_downHitInfo.InRowCell && l_downHitInfo.HitTest == GridHitTest.RowCell
			   &&(l_downHitInfo.Column.Equals(col_surplus_foreign_amount)||l_downHitInfo.Column.Equals(col_surplus_native_amount)))
			{
				int l_rowHandle = BGV_List.GetDataSourceRowIndex(l_downHitInfo.RowHandle);
				int l_colHandle = l_downHitInfo.Column.ColumnHandle;
				if (l_rowHandle<0 || l_colHandle<0) return;
				
				//判断重复执行
				string l_strCellIn = l_colHandle.ToString() + ":" + l_rowHandle.ToString();
				//如果是同一个单元格，则忽略
				if (m_strLastCell.Equals(l_strCellIn))
				{
					return;
				}
				else
				{
					m_strLastCell = l_strCellIn;
				}
				
				DataTable l_tbSource = BGV_List.GridControl.DataSource as DataTable;
				string l_strField = l_downHitInfo.Column.FieldName;

				//构造浮窗上显示的内容
				string l_strHint = "期末=期初+销售发票-收款-采购发票+付款-调差";

				Rectangle l_cellRect = l_gh.ViewInfo.GetGridCellInfo(l_downHitInfo).CellValueRect;
				Point l_pt = BGV_List.GridControl.PointToScreen(new Point(l_cellRect.X, l_cellRect.Y + l_cellRect.Height + 1));
				
				l_pt = Control.MousePosition;
				l_pt = l_frmOwner.PointToClient(l_pt);
				if (BGV_List.OptionsHint.ShowCellHints) BGV_List.OptionsHint.ShowCellHints = false;
				ShowHint(l_strHint, l_frmOwner, l_pt);
			}
			else if (l_downHitInfo.HitTest != GridHitTest.RowCell)
			{
				m_strLastCell = "";
				m_strLastHint = "";
				m_Hint.Hide(BGV_List.GridControl);
			}
		}
		
		void ShowHint(string a_strHint, IWin32Window a_winHandle, Point a_Point)
		{
			if (string.IsNullOrEmpty(a_strHint)) return;
			a_Point.Y += 24;
			Size l_size = TextRenderer.MeasureText(a_strHint, SystemFonts.MenuFont);
			if (Control.MousePosition.X > (Screen.PrimaryScreen.WorkingArea.Right - l_size.Width - 24) ||
			    Control.MousePosition.Y > (Screen.PrimaryScreen.WorkingArea.Bottom - l_size.Height -24))
			{
				a_Point = Control.FromHandle(a_winHandle.Handle).PointToClient(Control.MousePosition);
				a_Point.X -= (l_size.Width + 24);
				a_Point.Y -= l_size.Height + 24;
			}
			
			m_Hint.Show(a_strHint, a_winHandle, a_Point, 10000);
			m_strLastHint = a_strHint;
		}
		#endregion
		
	}
}