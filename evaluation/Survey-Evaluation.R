library(varhandle)
library(lme4);library(lsmeans);library(lmerTest);library(MASS)
library(gdata);library(ggplot2)
####################Read in data
d=read.xls("questions.xlsx",header=TRUE)
d;attach(d)
head(d)

table1=table(Device_assessment,as.factor(Q1))
table2=table(Device_assessment,as.factor(Q2))
table3=table(Device_assessment,as.factor(Q3))
table4=table(Device_assessment,as.factor(Q4))
table5=table(Device_assessment,as.factor(Q5))
table6=table(Device_assessment,as.factor(Q6))

chiQ1 = chisq.test(table1)
chiQ2 = chisq.test(table2)
chiQ3 = chisq.test(table3)
chiQ4 = chisq.test(table4)
chiQ5 = chisq.test(table5)
chiQ6 = chisq.test(table6)

chiQ1$p.value
chiQ2$p.value
chiQ3$p.value
chiQ4$p.value
chiQ5$p.value
chiQ6$p.value