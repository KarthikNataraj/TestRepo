

var initParam={
	pattern:"[%-5p] - { %d{dd MMM yyyy HH:mm:ss,SSS} } - %m",
    Threshold: "INFO",
    initLogger:function(){initLogger(initParam.pattern,initParam.Threshold);}
}
var hex = {
	logger:initParam,
  	log:log4javascript.getLogger()
	
}


	
function initLogger(pattern,Threshold)
{
	var threshold;
	switch(Threshold)
		{
			case "DEBUG":
				threshold=log4javascript.Level.DEBUG;
			break;
			case "INFO":
				threshold=log4javascript.Level.INFO;
			break;
			case "TRACE":
				threshold=log4javascript.Level.TRACE;
			break;
			case "WARN":
				threshold=log4javascript.Level.WARN;
			break;
			case "ERROR":
				threshold=log4javascript.Level.ERROR;
			break;
			case "FATAL":
				threshold=log4javascript.Level.FATAL;
			break;
			case "OFF":
				threshold=log4javascript.Level.OFF;
			break;
			case "ALL":
				threshold=log4javascript.Level.ALL;
			break;
		}
	
	
	var BrowserConsoleAppender = new log4javascript.BrowserConsoleAppender();
	var layout = new log4javascript.PatternLayout(pattern);
	hex.log.addAppender(BrowserConsoleAppender);
	BrowserConsoleAppender.setThreshold(threshold);
	BrowserConsoleAppender.setLayout(layout);
	
}
	
	
	hex.logger.pattern="[%-5p] - { %d{dd MMM yyyy HH:mm:ss,SSS} } - %m";
	hex.logger.Threshold="WARN";
	hex.logger.initLogger();
	
	