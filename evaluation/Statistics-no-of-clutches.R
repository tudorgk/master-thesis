####################Load packages
#install.packages("lme4");install.packages("lsmeans");install.packages("lmerTest");
#install.packages("MASS");install.packages("Rmisc");
#install.packages("dplyr");install.packages("Rmisc");
library(lme4);library(lsmeans);library(lmerTest);library(MASS)
library(ggplot2); library(Rmisc);
library(dplyr)

####################Read in data
d=read.csv("data.csv",header=TRUE)
d;attach(d)
head(d)

####################Prepare data
#Transform data - Because the residuals was not normally distributed in the model
#INFO: page 231, 0.5 was added to all (Sokal and Rohlf) and a natural log transformation was done - because lambda = 0 in boxcox
nr_of_clutches<-log(d$nr_of_clutches+0.5)

###################Finding the right model
#Quick look at the data
d$combine_trial_dist<-as.factor(paste(trial_type,target_distance,sep=""))	
plot(d$combine_trial_dist,nr_of_clutches)		#Corresponds to model 3 (without user_id)
plot(trial_type,nr_of_clutches)			#Corresponds to a model with only trial type

#Model reduction
model4<-lmer(nr_of_clutches ~ trial_type*target_distance + direction_axis*trial_type + task_number + (1|user_id))		#Repeated measures factorial ANCOVA (mixed effects model) with interaction (user is random effect)

#Test of interaction
model3<-lmer(nr_of_clutches ~ trial_type*target_distance + direction_axis + task_number + (1|user_id))		#Repeated measures factorial ANCOVA (mixed effects model) with interaction (user is random effect)
anova(model3,model4)

model2<-lmer(nr_of_clutches ~ trial_type + target_distance + direction_axis + task_number + (1|user_id))
	anova(model2,model3)

	#Because the models are significantly different (p<0.05) (the interaction is significant so we cannot exclude it),
	#so we carry on with model 3.

#Test of task_number
model1<-lmer(nr_of_clutches ~ trial_type*target_distance + direction_axis + (1|user_id))		#ANOVA
	anova(model3,model1)

	#Because the models are NOT significantly different (p<0.05) (the task_number is NOT significant),
	#so we carry on with model 1.

#Test of direction_axis
model0<-lmer(nr_of_clutches ~ trial_type*target_distance + (1|user_id))		#ANOVA
	anova(model1,model0)

	#Because the models are significantly different (p<0.05) (the direction_axis IS significant),
	#so model 1 is our final model.


####################Hypothesis testing - Test if there is a difference between trial types
#p value for interaction
model1.pval<-lmer(nr_of_clutches ~ trial_type+target_distance + direction_axis + (1|user_id))		#ANOVA
	anova(model1,model1.pval)
	
#From above we saw that the interaction was significant. With LSM we can test the difference for each level
LSM<-lsmeans::lsmeans(model1,~trial_type*target_distance)	#Estimates for each level
	T<-contrast(LSM, "pairwise",adjust="none");T		#Estiates for the contrasts
	CIs<-confint(T)							#95% CIs for the contrasts
	LSM_summary<-summary(LSM)
	#Back transformation
	LSM_summary$lsmean <- exp(LSM_summary$lsmean)-0.5
	LSM_summary$lower.CL <- exp(LSM_summary$lower.CL)-0.5
	LSM_summary$upper.CL <- exp(LSM_summary$upper.CL)-0.5
	LSM_summary$target_distance <- factor(LSM_summary$target_distance, levels = rev(levels(LSM_summary$target_distance)))
	ggplot(LSM_summary, aes(x=target_distance, y=lsmean, colour=trial_type, group=trial_type)) + 
	  geom_errorbar(aes(ymin=lower.CL, ymax=upper.CL), width=.1, 
	                position=position_dodge(0.00)) +
	  scale_colour_hue(name="Interaction Method")  +     # Set legend title
	  scale_linetype_discrete(name="Interaction Method") +
	  xlab("Target Distance") +
	  ylab("Number of clutches") +
	  geom_line() +
	  geom_point()
#Read about lsmeans: https://cran.r-project.org/web/packages/lsmeans/vignettes/using-lsmeans.pdf
	
#####################Plotting directions for the model
LSM_with_direction<-lsmeans::lsmeans(model1,~trial_type*target_distance*direction_axis)	#Estimates for each level
T_with_direction<-contrast(LSM_with_direction, "pairwise",adjust="none");		#Estiates for the contrasts
LSM_summary<-summary(LSM_with_direction)

#capture.output(T_with_direction,file=paste("Direction Contrast",".doc",sep=""))

#Back transformation
LSM_summary$lsmean <- exp(LSM_summary$lsmean)-0.5
LSM_summary$lower.CL <- exp(LSM_summary$lower.CL)-0.5
LSM_summary$upper.CL <- exp(LSM_summary$upper.CL)-0.5
LSM_summary$target_distance <- factor(LSM_summary$target_distance, levels = rev(levels(LSM_summary$target_distance)))

#Subset
midair.subset<-subset(LSM_summary,trial_type=="midair")
touch.subset<-subset(LSM_summary,trial_type=="touch+inertia")

#plot touch
ggplot(touch.subset, aes(x=target_distance, y=lsmean, colour=direction_axis, group=interaction(trial_type,direction_axis))) + 
  geom_errorbar(aes(ymin=lower.CL, ymax=upper.CL), width=.1, 
                position=position_dodge(0.2)) +
  scale_colour_hue(name="Target Direction")  +     # Set legend title
  scale_linetype_discrete(name="Target Direction") +
  xlab("Target Distance") +
  ylab("Number of clutches") +
  geom_line(position=position_dodge(0.2)) +
  geom_point(position=position_dodge(0.2))

  #ggtitle("Number of clutches against distance stratified by direction for the touch+inertia interaction method")

#plot midair
ggplot(midair.subset, aes(x=target_distance, y=lsmean, colour=direction_axis, group=interaction(trial_type,direction_axis))) + 
  geom_errorbar(aes(ymin=lower.CL, ymax=upper.CL), width=.1, 
                position=position_dodge(0.2)) +
  scale_colour_hue(name="Target Direction")  +     # Set legend title
  scale_linetype_discrete(name="Target Direction") +
  xlab("Target Distance") +
  ylab("Number of clutches") +
  geom_line(position=position_dodge(0.2)) +
  geom_point(position=position_dodge(0.2))
#ggtitle("Number of clutches against distance stratified by direction for the mid-air interaction method")
####################Model validation
par(mfrow=c(2,2)) 	#For more plots in one window
plot(d$nr_of_clutches,main = "Plot of number of interactions before log tranformation", ylab = "Number of interactions")
plot(nr_of_clutches,main = "Plot of number of interactions after log tranformation", ylab = "Number of interactions")
hist(d$nr_of_clutches,main = "Histogram of number of interactions before log tranformation", xlab = "Number of interactions")
hist(nr_of_clutches,main = "Histogram of number of interactions after log tranformation", xlab = "Number of interactions")
#Before transformation
model1_nontrans<-lmer(d$nr_of_clutches ~ trial_type*target_distance + (1|user_id))	
	residuals_nontrans<-residuals(model1_nontrans)
	par(mfrow=c(2,3)) 	#For more plots in one window
	plot(fitted(model1_nontrans),residuals_nontrans,main = "Scatterplot of residuals before transformation", ylab = "Residuals before transformation", xlab = "Fitted model values");  abline(0,0)				#Test of the variance homogeneity
	hist(residuals_nontrans,main = "Histogram of residuals before transformation", xlab = "Residuals before transformation")										#Test of the normal distribution with histogram
	qqnorm(residuals(model1_nontrans),main = "Normal QQ-plot before transformation"); abline(mean(residuals_nontrans),sd(residuals_nontrans))	#Test of the normal distribution with QQ-plot

#Which transformation may be appropriate - using boxcox plot
#boxcox(d$nr_of_clutches ~ trial_type*target_distance, )		#Before transformation, lambda=0, meaning XXXX 

#After transformation
	residuals<-residuals(model1)
	plot(fitted(model1),residuals,main = "Scatterplot of residuals after transformation", ylab = "Residuals after transformation", xlab = "Fitted model values");  abline(0,0)	#Test of the variance homogeneity
	hist(residuals,main = "Histogram of residuals after transformation", xlab = "Residuals after transformation")						#Test of the normal distribution with histogram
	qqnorm(residuals(model1),main = "Normal QQ-plot after transformation"); abline(mean(residuals),sd(residuals))	#Test of the normal distribution with QQ-plot

#Which transformation may be appropriate - using boxcox plot 
#boxcox(nr_of_clutches ~ trial_type*target_distance)		#After transformation, lambda=1, meaning XXXX 


####################Getting estimates - backtransformed
#Transform and backtransform original data 
cbind(nr_of_clutches,log(d$nr_of_clutches+0.5),d$nr_of_clutches,exp(nr_of_clutches)-0.5)

#Backtransform estimates for contrasts
#The contrasts
names(CIs) #Get the names for extracting data form the output
	Names<-CIs$contrast[c(1,10,15)]
	Est<-CIs$estimate[c(1,10,15)];	Est<-exp(Est)-0.5
	LL<-CIs$lower.CL[c(1,10,15)];		LL<-exp(LL)-0.5
	UL<-CIs$upper.CL[c(1,10,15)];		UL<-exp(UL)-0.5
	matrix =cbind(as.vector(Names),Est,LL,UL)			#Note, 0 is NOT included in the 95% CIs

#The levels (FOR BAR PLOT - TO DO - USE LSM - REMEMBER TO BACKTRANSFORM)
tabbedMeans <- tapply(as.numeric(as.vector(Q_dataframe$b)), list(Q_dataframe$a,
                                                                 Q_dataframe$q),
                      function(x) c(x = x))
tabbedLow <- tapply(as.numeric(as.vector(Q_dataframe$c)), list(Q_dataframe$a,
                                                               Q_dataframe$q),
                    function(x) c(x = x))
tabbedHigh <- tapply(as.numeric(as.vector(Q_dataframe$d)), list(Q_dataframe$a,
                                                                Q_dataframe$q),
                     function(x) c(x = x))


####################Plot according to model3
d$num_combine_trial_dist<-ifelse(d$combine_trial_dist=="midairlong",1,d$combine_trial_dist)
	d[c("num_combine_trial_dist","combine_trial_dist")]		#Check that it is correct
plot(d$num_combine_trial_dist,d$nr_of_clutches,
	main="XXXX", ylab="nr of clutches", xlab="trial type 1-3, midair long, median, short, and type 4-5, touch long, median, short",
	,col=user_id,pch=18)		#Choose symbol (pch) here: http://www.sthda.com/sthda/RDoc/figure/graphs/r-plot-pch-symbols-points-in-r.png
plot(d$num_combine_trial_dist,nr_of_clutches,
	main="XXXX", ylab="log(nr of clutches) - PLEASE NOTE THIS IS LOG", xlab="trial type 1-3, midair long, median, short, and type 4-5, touch long, median, short",
	,col=user_id,pch=18)

####################Export results
capture.output(CIs,file=paste("NAME OF THE DOCUMENT",".doc",sep=""))




