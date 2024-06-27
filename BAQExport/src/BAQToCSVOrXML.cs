  //using Ice.Lib.SharedUtilities.ImportExport;
  //using System.IO;
  //using System.Text;


  // Return data as requested
  Func<string, string, string> ReturnTextOrBytes = (stringData, tOrB) =>
  {
      // default
      string returnData = stringData;
  
      if(tOrB.ToUpper() == "BYTES")
      {
          returnData = Convert.ToBase64String( Encoding.UTF8.GetBytes(stringData) );
      }

      return returnData;
  };



  try
  {
      DataSet ds = null;
    
      CallService<DynamicQuerySvcContract>(dq=>
      {
          ds = dq.ExecuteByID(baqID, new QueryExecutionTableset());
      });
    
      DataTable dataTable = ds.Tables[0];
      
      // Output Format = XML
      if(outputFormat.ToUpper() == "XML")
      {
          StringWriter writer = new StringWriter();
      
          dataTable.WriteXml(writer, XmlWriteMode.IgnoreSchema, false);

          string tempData = writer.ToString();

          //Make outputLabels work with XML
          if(outputLabelsXML == true && outputFormat.ToUpper() == "XML")
          {
              CallService<DynamicQuerySvcContract>(dynamicQuery =>
              {
                  DynamicQueryTableset dqTS = dynamicQuery.GetByID(baqID);
                  
                  Guid subQueryID = dqTS.QuerySubQuery.Where(x => x.Type == "TopLevel").FirstOrDefault().SubQueryID;
                  
                  foreach(QueryFieldRow qfr in dqTS.QueryField.Where(x => x.SubQueryID == subQueryID))
                  {
                      tempData = tempData.Replace($"{qfr.TableID}_{qfr.FieldName}",  System.Security.SecurityElement.Escape(qfr.FieldLabel).Replace(" ", "_")  );
                  }
                  
              });
          }

          data = ReturnTextOrBytes(tempData, textOrBytes);
          
          return;
      }




      // Output Format = CSV (Default)
      StringBuilder stringBuilder = new StringBuilder();
      
      string headerLine = "";
      
      if(showHeadersOnCSV == true && showColumnNamesInsteadofLabelsCSV == true)
      {
          string[] columnArray = dataTable.Columns.Cast<DataColumn>().Select(x => $"\"{x.ColumnName}\"").ToArray();
          
          headerLine = CsvWriter.GetCSVLine(columnArray);
      }

      if(showHeadersOnCSV == true && showColumnNamesInsteadofLabelsCSV == false)
      {
          CallService<DynamicQuerySvcContract>(dynamicQuery =>
          {
              DynamicQueryTableset dqTS = dynamicQuery.GetByID(baqID);
              
              Guid subQueryID = dqTS.QuerySubQuery.Where(x => x.Type == "TopLevel").FirstOrDefault().SubQueryID;

              string[] columnArray = dqTS.QueryField.Where(x => x.SubQueryID == subQueryID).Select(x => $"\"{x.FieldLabel}\"").ToArray();
    
    
              headerLine = CsvWriter.GetCSVLine(columnArray);

          });
      }

      
      if(showHeadersOnCSV == true)
      {
          stringBuilder.AppendLine(headerLine);
      }



      foreach(DataRow dataRow in dataTable.Rows)
      {
          string[] stringArray = dataRow.ItemArray.Select(x => x.ToString()).ToArray();
          
          stringBuilder.AppendLine(CsvWriter.GetCSVLine(stringArray));
      }

      // Text Delimiter is not a Comma - :(
      if(textDelim != ",")
      {
          string tempData = stringBuilder.ToString();
          stringBuilder.Clear();
      
          using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(tempData)))
          {
              CsvReader reader = new CsvReader(ms);
              
              reader.Separator = ',';
    
              string[] line = null;
              
              do
              {
                  line = null;
                  line = reader.GetCSVLine();
                  if(line != null) stringBuilder.AppendLine( String.Join(textDelim, line) );
              }            
              while (line != null);
          }
      }
      
      data = ReturnTextOrBytes(stringBuilder.ToString(), textOrBytes);
      
  }
  catch (Exception ex)
  {
      message = ex.Message;
  }
  
  
  
  
 