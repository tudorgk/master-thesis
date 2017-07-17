####Fatique Q3

####################Load packages
#install.packages("lme4");install.packages("lsmeans");install.packages("lmerTest");install.packages("MASS")
#install.packages("gdata");install.packages("ggplot2");install.packages("formattable");install.packages("varhandle");
library(varhandle)
library(lme4);library(lsmeans);library(lmerTest);library(MASS)
library(gdata);library(ggplot2)
####################Read in data
d=read.xls("questions.xlsx",header=TRUE)
d;attach(d)
head(d)

get_model_estimate_matrix <- function (question) {
  #Hypothesis testing
  model3<-lmer(question ~ Device_assessment + (1|User_Id))		#Repeated measures factorial ANOVA (mixed effects model)
  model2<-lmer(question ~ (1|User_Id))
  anova_result = anova(model2,model3)
  
  #Read about lsmeans: https://cran.r-project.org/web/packages/lsmeans/vignettes/using-lsmeans.pdf
  LSM<-lsmeans::lsmeans(model3,~Device_assessment)		#Estimates for each level
  Get<-confint(LSM)
  
  q<-as.vector(deparse(substitute(question)))
  a<-as.vector(Get$Device_assessment)
  b<-as.vector(Get$lsmean)
  c<-as.vector(Get$lower.CL)
  d<-as.vector(Get$upper.CL)
  e<-as.vector(anova_result$`Pr(>Chisq)`[2])
  matrix<-cbind(q,a,b,c,d,e)
  return(matrix)
}

Q1_matrix = get_model_estimate_matrix(Q1)
Q2_matrix = get_model_estimate_matrix(Q2)
Q3_matrix = get_model_estimate_matrix(Q3)
Q4_matrix = get_model_estimate_matrix(Q4)
Q5_matrix = get_model_estimate_matrix(Q5)
Q6_matrix = get_model_estimate_matrix(Q6)

Q_matrix = rbind(Q1_matrix,Q2_matrix,Q3_matrix,Q4_matrix,Q5_matrix,Q6_matrix)
Q_dataframe = as.data.frame(Q_matrix)

tabbedMeans <- tapply(as.numeric(as.vector(Q_dataframe$b)), list(Q_dataframe$a,
                                                                 Q_dataframe$q),
                      function(x) c(x = x))
tabbedLow <- tapply(as.numeric(as.vector(Q_dataframe$c)), list(Q_dataframe$a,
                                                               Q_dataframe$q),
                    function(x) c(x = x))
tabbedHigh <- tapply(as.numeric(as.vector(Q_dataframe$d)), list(Q_dataframe$a,
                                                                Q_dataframe$q),
                     function(x) c(x = x))
barCenters <- barplot(height = tabbedMeans,
                      beside = TRUE, las = 1,
                      ylim = c(0, 7),
                      main = "User Questionnaire Results",
                      ylab = "Mean Rating",
                      axes = TRUE,
                      border = "black",
                      legend.text = TRUE,
                      args.legend = list(title = "Interaction Methods", 
                                         x = "topright",
                                         cex = .7))
#segments(barCenters, tabbedLow, barCenters,
#         tabbedHigh, lwd = 0.5)

#arrows(barCenters, tabbedLow, barCenters,
#       tabbedHigh, lwd = 0.5, angle = 90,
#       code = 3, length = 0.05)
#text(x=barCenters , y= tabbedHigh + 0.2, labels=paste("p=",format(round(unfactor(Q_dataframe$e), 3), nsmall = 3)), xpd=TRUE, cex=0.7)

