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
      // Make file name unique
      Guid findGuid = Guid.NewGuid();
      string findFilename = $"{baqID}_{findGuid.ToString()}.{outputFormat.ToLower()}";
     
      //Force headers     
      if(showHeadersOnCSV == true && outputFormat.ToUpper() == "CSV") outputLabels = true;
     
     
      CallService<DynamicQueryExportSvcContract>(dynamicQueryExport =>
      {
          DynamicQueryExportTableset dynamicQueryExportTableset = dynamicQueryExport.GetNewParameters();
      
          DynQueryExpParamRow dynQueryExpParamRow = dynamicQueryExportTableset.DynQueryExpParam.FirstOrDefault();
          
          dynQueryExpParamRow.QueryID = baqID;
          dynQueryExpParamRow.ExportFormat = outputFormat.ToUpper();
          dynQueryExpParamRow.ExportFilename =  findFilename;
          dynQueryExpParamRow.TextDelim = textDelim;
          dynQueryExpParamRow.OutputLabels = outputLabels; //CSV Only here
      
          dynamicQueryExport.RunDirect(dynamicQueryExportTableset);
      });
      

      CallService<FileTransferSvcContract>(fileTransfer =>
      {
          byte[] csvDataBytes = fileTransfer.DownloadFile(Epicor.ServiceModel.Utilities.SpecialFolder.CompanyData, $"Processes\\{Session.UserID}\\{findFilename}");
          
          // Should we delete file?
          if(deleteFile)
          {
              fileTransfer.FileDelete(Epicor.ServiceModel.Utilities.SpecialFolder.CompanyData, $"Processes\\{Session.UserID}\\{findFilename}");
          }
          
          string tempData = System.Text.Encoding.UTF8.GetString(csvDataBytes);

          //Make outputLabels work with XML
          if(outputLabels == true && outputFormat.ToUpper() == "XML")
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


          if(showHeadersOnCSV == true && showColumnNamesInsteadofLabelsCSV == true && outputFormat.ToUpper() == "CSV" )
          {
              if(outputLabels == true)
              tempData = tempData.Substring( tempData.IndexOf(Environment.NewLine) );
              
              CallService<DynamicQuerySvcContract>(dynamicQuery =>
              {
                  DynamicQueryTableset dqTS = dynamicQuery.GetByID(baqID);
                  
                  Guid subQueryID = dqTS.QuerySubQuery.Where(x => x.Type == "TopLevel").FirstOrDefault().SubQueryID;
    
                  
                  string headerLine = String.Join(textDelim, dqTS.QueryField.Where(x => x.SubQueryID == subQueryID).Select(x => $"\"{x.FieldName}\"").ToArray());
      
                  tempData = headerLine + Environment.NewLine + tempData;                  

              });
          }

          
          if(showHeadersOnCSV == false && outputLabels == true)
          {
              tempData = tempData.Substring( tempData.IndexOf(Environment.NewLine) );
          }


          data = ReturnTextOrBytes(tempData, textOrBytes);
          
      });
  }
  catch (Exception ex)
  {
      message = ex.Message;
  }
   