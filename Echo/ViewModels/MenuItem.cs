using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace Echo.ViewModels;
public class MenuItem 
{
	public string Name { get; set; }
	public string Description { get; set; }
	public string Icon { get; set; }
	public string ViewName { get; set; }
}