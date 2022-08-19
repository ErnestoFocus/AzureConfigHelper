using ConfigFromAzureToLocalConverter;
using Newtonsoft.Json;
using System.Drawing;
using System.Linq;
using System.Text;

var final = "";
var success = false;
var text = "";

Console.ForegroundColor = ConsoleColor.Yellow;

Console.WriteLine("┌─────────────────────────────────────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│  Hola soy un conversor de configs desde azure a local y viceversa                               │");
Console.WriteLine("│  Teneme paciencia, tal vez me rompa y me de amziedad                                            │");
Console.WriteLine("│  Si no te doy el resultado que esperás, cambia la modalidad (copy paste o ruta).                │");
Console.WriteLine("│  Aguanto hasta 3 niveles de configs (Pagos_BICE_llave), mas de eso no estoy testeado            │");
Console.WriteLine("│  Si tenes ganas de hacerme mejoras, bienvenidas sean! Espero servirte                           │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────────────────────────────────────┘");

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Desea ingresar el archivo pegandolo (copy paste)?: presione 1");
Console.WriteLine("O en una ruta local: presione 2?");
Console.ForegroundColor = ConsoleColor.White;
var key2 = Console.ReadKey();
Console.ForegroundColor = ConsoleColor.Green;
if (key2.Key == ConsoleKey.D1 || key2.Key == ConsoleKey.NumPad1)
{
	Console.WriteLine();
	Console.WriteLine("Pegue el texto y pulse enter");
	byte[] inputBuffer = new byte[8192];
	Stream inputStream = Console.OpenStandardInput(inputBuffer.Length);
	Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length));
	Console.ForegroundColor = ConsoleColor.White;
	text = Console.ReadLine();
}
else if (key2.Key == ConsoleKey.D2 || key2.Key == ConsoleKey.NumPad2)
{
	Console.WriteLine();
	Console.WriteLine("Pegue la ruta al archivo (incluyendo formato) y pulse enter");
	Console.ForegroundColor = ConsoleColor.White;
	var path = Console.ReadLine();
	text = File.ReadAllText(path);
}
else
{
	Console.WriteLine();
	Console.WriteLine("1 o 2 te dije");
	throw new Exception("1 o 2 te dije");
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Modo para traer de azure a archivo local: presione 1");
Console.WriteLine("O de local hacia azure: presione 2");
Console.ForegroundColor = ConsoleColor.White;
var key1 = Console.ReadKey();
Console.ForegroundColor = ConsoleColor.Green;

if (key1.Key == ConsoleKey.D1 || key1.Key == ConsoleKey.NumPad1)
{
	List<ModelInAzure> obj;
	try
	{
		obj = JsonConvert.DeserializeObject<List<ModelInAzure>>(text);
	}
	catch (Exception)
	{
		Console.WriteLine();
		Console.WriteLine("El texto es inválido");
		throw new Exception("El texto es inválido");
	}
	Console.WriteLine();
	Console.WriteLine("Modo para Functions (local.settings): presione 1");
	Console.WriteLine("O para apps (appsettings): presione 2");
	Console.ForegroundColor = ConsoleColor.White;
	var key = Console.ReadKey();
	Console.ForegroundColor = ConsoleColor.Green;
	if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1)
	{
		final = MethodsFromAzure.CreateFunctionsLocalSettings(obj);
		success = true;
	}
	else if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2)
	{
		final = MethodsFromAzure.CreateAppsettings(obj);
		success = true;
	}
	else
	{
		final = "Opción Invalida";
		success = false;
	}
}
else if (key1.Key == ConsoleKey.D2 || key1.Key == ConsoleKey.NumPad2)
{
	bool slotSetting;


	Console.WriteLine();
	Console.WriteLine("Modo para entornos no productivos o sin slots (dev, qa, etc): presione 1");
	Console.WriteLine("O para entornos productivos o con slots: presione 2");
	Console.ForegroundColor = ConsoleColor.White;
	var key3 = Console.ReadKey();
	Console.ForegroundColor = ConsoleColor.Green;

	if (key3.Key == ConsoleKey.D1 || key3.Key == ConsoleKey.NumPad1) slotSetting = false;
	else if (key3.Key == ConsoleKey.D2 || key3.Key == ConsoleKey.NumPad2) slotSetting = true;
	else
	{
		Console.WriteLine();
		Console.WriteLine("1 o 2 te dije");
		throw new Exception("1 o 2 te dije");
	}

	Console.WriteLine();
	Console.WriteLine("Modo desde Functions (local.settings): presione 1");
	Console.WriteLine("O para apps (appsettings): presione 2");
	Console.ForegroundColor = ConsoleColor.White;
	var key4 = Console.ReadKey();
	Console.ForegroundColor = ConsoleColor.Green;

	var obj = new Dictionary<string, dynamic>();
	var minified = MethodsFromLocal.MinifyJson(text);

	try
	{
		obj = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(minified);
	}
	catch (Exception)
	{
		throw new Exception("El texto es inválido");
	}

	if (key4.Key == ConsoleKey.D1 || key4.Key == ConsoleKey.NumPad1)
	{
		obj = obj.Where(x => x.Key == "Values")
				 .ToDictionary(x => x.Key, x => x.Value);
	}
	else if (key4.Key != ConsoleKey.D2 && key4.Key != ConsoleKey.NumPad2)
	{
		Console.WriteLine();
		Console.WriteLine("1 o 2 te dije");
		throw new Exception("1 o 2 te dije");
	}

	final = MethodsFromLocal.Create(obj,slotSetting);	
	success = true;
}
else
{
	final = "Opción Invalida";
	success = false;
}

if (success)
{
	TextCopy.ClipboardService.SetText(final);
	Console.WriteLine();
	Console.WriteLine("Copiado al portapapeles!");
}
Console.WriteLine();
Console.WriteLine("Presione cualquier tecla para salir");
Console.ReadKey();

/// <summary>
/// Helpers
/// </summary>
public static class MethodsFromAzure
{
	public static string CreateFunctionsLocalSettings(List<ModelInAzure> obj)
	{
		var final = $"{{\n  \"IsEncrypted\": false,\n  \"Values\": {{\n\t \"FUNCTIONS_WORKER_RUNTIME\": \"dotnet\",\n";
		var count = 0;
		foreach (var item in obj)
		{
			final += $"\t\t\"{item.name}\" : \"{item.value}\"";
			count++;
			if (count != obj.Count) final += ",\n";
		}
		final += $"\n\t}}\n}}";
		return final;
	}

	public static string CreateAppsettings(List<ModelInAzure> obj)
	{
		var final = "";
		var firstObjects = new List<Dictionary<string, string>>();
		var secondObjects = new Dictionary<string, Dictionary<string, string>>();
		var thirdObjects = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
		foreach (var item in obj)
		{
			var names = item.name.Split("__");

			if (names.Count() == 1)
				firstObjects.Add(new Dictionary<string, string>
				{
					{
						names[0], item.value
					}
				});

			else if (names.Count() == 2)
			{
				var existingSecondObject = secondObjects.FirstOrDefault(x => x.Key == names[0]);

				if (!string.IsNullOrEmpty(existingSecondObject.Key))
					existingSecondObject.Value.Add(names[1], item.value);
				else
				{
					secondObjects.Add(names[0], new Dictionary<string, string>
					{{
						names[1], item.value
					}});
				}
			}

			else if (names.Count() == 3)
			{
				var existingThirdObject = thirdObjects.FirstOrDefault(x => x.Key == names[0]);

				if (!string.IsNullOrEmpty(existingThirdObject.Key))
				{
					var existingThirdObjectSecondScale = existingThirdObject.Value.FirstOrDefault(x => x.Key == names[1]);
					if (!string.IsNullOrEmpty(existingThirdObjectSecondScale.Key))
					{
						existingThirdObjectSecondScale.Value.Add(names[2], item.value);
					}
					else
					{
						existingThirdObject.Value.Add(names[1], new Dictionary<string, string>
						{{
							names[2], item.value
						}});
					}
				}
				else
				{
					thirdObjects.Add(names[0], new Dictionary<string, Dictionary<string, string>>
					{{
						names[1], new Dictionary<string, string>
						{{
							names[2], item.value
						}}
					}});
				}
			}
		}

		if (firstObjects.Any())
		{
			final += $"{{\n ";
			for (int i = 0; i < firstObjects.Count; i++)
			{
				final += $"\"{firstObjects[i].First().Key}\": \"{firstObjects[i].First().Value}\"";
				if (i + 1 != firstObjects.Count) final += ",\n ";
			}
		}

		if (secondObjects.Any())
		{
			if (firstObjects.Any()) final += ",\n ";
			else final += $"{{\n ";
			var firstScaleCount = 0;
			foreach (var item in secondObjects)
			{
				final += $"\"{item.Key}\" : {{\n";
				var secondScaleCount = 0;
				foreach (var secondScale in item.Value)
				{
					final += $"\t\"{secondScale.Key}\" : \"{secondScale.Value}\"";
					secondScaleCount++;
					if (secondScaleCount != item.Value.Count) final += ",\n ";
				}
				final += $"\n}}";
				firstScaleCount++;
				if (firstScaleCount != secondObjects.Count) final += ",\n ";
			}
		}

		if (thirdObjects.Any())
		{
			if (firstObjects.Any() || secondObjects.Any()) final += ",\n ";
			else final += $"{{\n ";

			var firstScaleCount = 0;
			foreach (var item in thirdObjects)
			{
				final += $"\"{item.Key}\" : {{\n";
				var secondScaleCount = 0;
				foreach (var secondScale in item.Value)
				{
					final += $"\t\"{secondScale.Key}\" : {{\n";
					var thirdScaleCount = 0;
					foreach (var thirdScale in secondScale.Value)
					{
						final += $"\t\t\"{thirdScale.Key}\" : \"{thirdScale.Value}\"";
						thirdScaleCount++;
						if (thirdScaleCount != secondScale.Value.Count) final += ",\n ";
					}
					final += $"\n\t\t}}";
					secondScaleCount++;
					if (secondScaleCount != item.Value.Count) final += ",\n ";
				}
				final += $"\n\t}}";
				firstScaleCount++;
				if (firstScaleCount != thirdObjects.Count) final += ",\n ";
			}
		}
		final += "\n}";
		return final;
	}
}
public static class MethodsFromLocal
{
	public static string Create(Dictionary<string, dynamic> obj, bool slotSetting)
	{
		var result = new List<ModelInAzure>();
		foreach (var item in obj)
		{
			string valStringDict1 = item.Value.ToString();
			if (valStringDict1.StartsWith("{"))
			{
				var dict2 = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(MethodsFromLocal.MinifyJson(valStringDict1));
				foreach (var kvpDict2 in dict2)
				{
					if (kvpDict2.Key == "FUNCTIONS_WORKER_RUNTIME") continue;
					string valStringDict2 = kvpDict2.Value.ToString();
					if (valStringDict2.StartsWith("{"))
					{
						var dict3 = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(MethodsFromLocal.MinifyJson(valStringDict2));
						foreach (var kvpDict3 in dict3)
						{
							string valStringDict3 = kvpDict2.Value.ToString();
							result.Add(new ModelInAzure
							{
								name = $"{item.Key}__{kvpDict2.Key}__{kvpDict3.Key}",
								value = kvpDict3.Value,
								slotSetting = slotSetting
							});
						}
					}
					else
					{
						var model = new ModelInAzure
						{
							value = kvpDict2.Value,
							slotSetting = slotSetting
						};
						if (item.Key == "Values") model.name = kvpDict2.Key;
						else model.name = $"{item.Key}__{kvpDict2.Key}";
						result.Add(model);
					}
				}
			}
			else
			{
				result.Add(new ModelInAzure
				{
					name = item.Key,
					value = item.Value,
					slotSetting = slotSetting
				});
			}
		}
		return JsonConvert.SerializeObject(result);
	}

	public static string MinifyJson(string input)
	{
		var ser = JsonConvert.DeserializeObject(input);
		return JsonConvert.SerializeObject(ser);
	}
}